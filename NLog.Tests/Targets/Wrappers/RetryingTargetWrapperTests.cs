
namespace NLog.UnitTests.Targets.Wrappers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using NUnit.Framework;
	using NLog.Common;
	using NLog.Internal;
	using NLog.Targets;
	using NLog.Targets.Wrappers;

	[TestFixture]
	public class RetryingTargetWrapperTests : NLogTestBase
	{
		[Test]
		public void RetryingTargetWrapperTest1()
		{
			var target = new MyTarget();
			var wrapper = new RetryingTargetWrapper()
			{
				WrappedTarget = target,
				RetryCount = 10,
				RetryDelayMilliseconds = 1,
			};

			wrapper.DeepInitialize(CommonCfg);

			var exceptions = new List<Exception>();

			var events = new []
			{
				new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger2", "Hello").WithContinuation(exceptions.Add),
			};

			wrapper.WriteAsyncLogEvents(events);

			// make sure all events went through
			Assert.AreEqual(3, target.Events.Count);
			Assert.AreSame(events[0].LogEvent, target.Events[0]);
			Assert.AreSame(events[1].LogEvent, target.Events[1]);
			Assert.AreSame(events[2].LogEvent, target.Events[2]);

			Assert.AreEqual(events.Length, exceptions.Count, "Some continuations were not invoked.");

			// make sure there were no exception
			foreach (var ex in exceptions)
			{
				Assert.IsNull(ex);
			}
		}

		[Test]
		public void RetryingTargetWrapperTest2()
		{
			var target = new MyTarget()
			{
				ThrowExceptions = 6,
			};

			var wrapper = new RetryingTargetWrapper()
			{
				WrappedTarget = target,
				RetryCount = 4,
				RetryDelayMilliseconds = 1,
			};

			wrapper.DeepInitialize(CommonCfg);

			var exceptions = new List<Exception>();

			var events = new []
			{
				new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger2", "Hello").WithContinuation(exceptions.Add),
			};

			var internalLogOutput = RunAndCaptureInternalLog(() => wrapper.WriteAsyncLogEvents(events), LogLevel.Trace);
			string expectedLogOutput = @"Warn Error while writing to 'MyTarget': System.InvalidOperationException: Some exception has ocurred.. Try 1/4
Warn Error while writing to 'MyTarget': System.InvalidOperationException: Some exception has ocurred.. Try 2/4
Warn Error while writing to 'MyTarget': System.InvalidOperationException: Some exception has ocurred.. Try 3/4
Warn Error while writing to 'MyTarget': System.InvalidOperationException: Some exception has ocurred.. Try 4/4
Warn Too many retries. Aborting.
Warn Error while writing to 'MyTarget': System.InvalidOperationException: Some exception has ocurred.. Try 1/4
Warn Error while writing to 'MyTarget': System.InvalidOperationException: Some exception has ocurred.. Try 2/4
";
			Assert.AreEqual(expectedLogOutput, internalLogOutput);

			// first event does not get to wrapped target because of too many attempts.
			// second event gets there in 3rd retry
			// and third event gets there immediately
			Assert.AreEqual(2, target.Events.Count);
			Assert.AreSame(events[1].LogEvent, target.Events[0]);
			Assert.AreSame(events[2].LogEvent, target.Events[1]);

			Assert.AreEqual(events.Length, exceptions.Count, "Some continuations were not invoked.");

			Assert.IsNotNull(exceptions[0]);
			Assert.AreEqual("Some exception has ocurred.", exceptions[0].Message);
			Assert.IsNull(exceptions[1]);
			Assert.IsNull(exceptions[2]);
		}

		public class MyTarget : Target
		{
			public MyTarget()
			{
				this.Events = new List<LogEventInfo>();
			}

			public List<LogEventInfo> Events { get; set; }

			public int ThrowExceptions { get; set; }

			protected override void Write(AsyncLogEventInfo logEvent)
			{
				if (this.ThrowExceptions-- > 0)
				{
					logEvent.Continuation(new InvalidOperationException("Some exception has ocurred."));
					return;
				}

				this.Events.Add(logEvent.LogEvent);
				logEvent.Continuation(null);
			}

			protected override void Write(LogEventInfo logEvent)
			{
			}
		}
	}
}
