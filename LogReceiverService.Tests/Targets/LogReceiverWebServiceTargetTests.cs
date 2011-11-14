
namespace NLog.UnitTests.Targets
{
	using System;
	using System.Collections.Generic;
	using NLog.Common;
	using NLog.LogReceiverService;
	using NUnit.Framework;
	using NLog.Config;
	using NLog.Targets;

	[TestFixture]
	public class LogReceiverWebServiceTargetTests : NLogTestBase
	{
		[Test]
		public void LogReceiverWebServiceTargetSingleEventTest()
		{
			var logger = LogManager.GetLogger("loggerName");
			var target = new MyLogReceiverWebServiceTarget();
			target.EndpointAddress = "http://notimportant:9999/";
			target.Parameters.Add(new MethodCallParameter("message", "${message}"));
			target.Parameters.Add(new MethodCallParameter("lvl", "${level}"));

			SimpleConfigurator.ConfigureForTargetLogging(target);
			logger.Info("message text");

			var payload = target.LastPayload;
			Assert.AreEqual(2, payload.LayoutNames.Count);
			Assert.AreEqual("message", payload.LayoutNames[0]);
			Assert.AreEqual("lvl", payload.LayoutNames[1]);
			Assert.AreEqual(3, payload.Strings.Count);
			Assert.AreEqual(1, payload.Events.Length);
			Assert.AreEqual("message text", payload.Strings[payload.Events[0].ValueIndexes[0]]);
			Assert.AreEqual("Info", payload.Strings[payload.Events[0].ValueIndexes[1]]);
			Assert.AreEqual("loggerName", payload.Strings[payload.Events[0].LoggerOrdinal]);
		}

		[Test]
		public void LogReceiverWebServiceTargetMultipleEventTest()
		{
			var target = new MyLogReceiverWebServiceTarget();
			target.EndpointAddress = "http://notimportant:9999/";
			target.Parameters.Add(new MethodCallParameter("message", "${message}"));
			target.Parameters.Add(new MethodCallParameter("lvl", "${level}"));

			var exceptions = new List<Exception>();

			var events = new[]
			{
				LogEventInfo.Create(LogLevel.Info, "logger1", "message1").WithContinuation(exceptions.Add),
				LogEventInfo.Create(LogLevel.Debug, "logger2", "message2").WithContinuation(exceptions.Add),
				LogEventInfo.Create(LogLevel.Fatal, "logger1", "message2").WithContinuation(exceptions.Add),
			};

			var configuration = new LoggingConfiguration();
			target.DeepInitialize(configuration);
			target.WriteAsyncLogEvents(events);

			// with multiple events, we should get string caching
			var payload = target.LastPayload;
			Assert.AreEqual(2, payload.LayoutNames.Count);
			Assert.AreEqual("message", payload.LayoutNames[0]);
			Assert.AreEqual("lvl", payload.LayoutNames[1]);

			// 7 strings instead of 9 since 'logger1' and 'message2' are being reused
			Assert.AreEqual(7, payload.Strings.Count);

			Assert.AreEqual(3, payload.Events.Length);
			Assert.AreEqual("message1", payload.Strings[payload.Events[0].ValueIndexes[0]]);
			Assert.AreEqual("message2", payload.Strings[payload.Events[1].ValueIndexes[0]]);
			Assert.AreEqual("message2", payload.Strings[payload.Events[2].ValueIndexes[0]]);

			Assert.AreEqual("Info", payload.Strings[payload.Events[0].ValueIndexes[1]]);
			Assert.AreEqual("Debug", payload.Strings[payload.Events[1].ValueIndexes[1]]);
			Assert.AreEqual("Fatal", payload.Strings[payload.Events[2].ValueIndexes[1]]);

			Assert.AreEqual("logger1", payload.Strings[payload.Events[0].LoggerOrdinal]);
			Assert.AreEqual("logger2", payload.Strings[payload.Events[1].LoggerOrdinal]);
			Assert.AreEqual("logger1", payload.Strings[payload.Events[2].LoggerOrdinal]);

			Assert.AreEqual(payload.Events[0].LoggerOrdinal, payload.Events[2].LoggerOrdinal);
		}


		[Test]
		public void LogReceiverWebServiceTargetMultipleEventWithPerEventPropertiesTest()
		{
			var target = new MyLogReceiverWebServiceTarget();
			target.IncludeEventProperties = true;
			target.EndpointAddress = "http://notimportant:9999/";
			target.Parameters.Add(new MethodCallParameter("message", "${message}"));
			target.Parameters.Add(new MethodCallParameter("lvl", "${level}"));

			var exceptions = new List<Exception>();

			var events = new[]
			{
				LogEventInfo.Create(LogLevel.Info, "logger1", "message1").WithContinuation(exceptions.Add),
				LogEventInfo.Create(LogLevel.Debug, "logger2", "message2").WithContinuation(exceptions.Add),
				LogEventInfo.Create(LogLevel.Fatal, "logger1", "message2").WithContinuation(exceptions.Add),
			};

			events[0].LogEvent.Properties["prop1"] = "value1";
			events[1].LogEvent.Properties["prop1"] = "value2";
			events[2].LogEvent.Properties["prop1"] = "value3";

			events[0].LogEvent.Properties["prop2"] = "value2a";

			var configuration = new LoggingConfiguration();
			target.DeepInitialize(configuration);
			target.WriteAsyncLogEvents(events);

			// with multiple events, we should get string caching
			var payload = target.LastPayload;

			// 4 layout names - 2 from Parameters, 2 from unique properties in events
			Assert.AreEqual(4, payload.LayoutNames.Count);
			Assert.AreEqual("message", payload.LayoutNames[0]);
			Assert.AreEqual("lvl", payload.LayoutNames[1]);
			Assert.AreEqual("prop1", payload.LayoutNames[2]);
			Assert.AreEqual("prop2", payload.LayoutNames[3]);

			Assert.AreEqual(12, payload.Strings.Count);

			Assert.AreEqual(3, payload.Events.Length);
			Assert.AreEqual("message1", payload.Strings[payload.Events[0].ValueIndexes[0]]);
			Assert.AreEqual("message2", payload.Strings[payload.Events[1].ValueIndexes[0]]);
			Assert.AreEqual("message2", payload.Strings[payload.Events[2].ValueIndexes[0]]);

			Assert.AreEqual("Info", payload.Strings[payload.Events[0].ValueIndexes[1]]);
			Assert.AreEqual("Debug", payload.Strings[payload.Events[1].ValueIndexes[1]]);
			Assert.AreEqual("Fatal", payload.Strings[payload.Events[2].ValueIndexes[1]]);

			Assert.AreEqual("value1", payload.Strings[payload.Events[0].ValueIndexes[2]]);
			Assert.AreEqual("value2", payload.Strings[payload.Events[1].ValueIndexes[2]]);
			Assert.AreEqual("value3", payload.Strings[payload.Events[2].ValueIndexes[2]]);

			Assert.AreEqual("value2a", payload.Strings[payload.Events[0].ValueIndexes[3]]);
			Assert.AreEqual("", payload.Strings[payload.Events[1].ValueIndexes[3]]);
			Assert.AreEqual("", payload.Strings[payload.Events[2].ValueIndexes[3]]);

			Assert.AreEqual("logger1", payload.Strings[payload.Events[0].LoggerOrdinal]);
			Assert.AreEqual("logger2", payload.Strings[payload.Events[1].LoggerOrdinal]);
			Assert.AreEqual("logger1", payload.Strings[payload.Events[2].LoggerOrdinal]);

			Assert.AreEqual(payload.Events[0].LoggerOrdinal, payload.Events[2].LoggerOrdinal);
		}

		public class MyLogReceiverWebServiceTarget : LogReceiverWebServiceTarget
		{
			public NLogEvents LastPayload;

			protected override bool OnSend(NLogEvents events, IEnumerable<AsyncLogEventInfo> asyncContinuations)
			{
				this.LastPayload = events;

				foreach (var ac in asyncContinuations)
				{
					ac.Continuation(null);
				}

				return false;
			}
		}
	}
}
