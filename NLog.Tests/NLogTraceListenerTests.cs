#define DEBUG
#define TRACE

using System;
using System.Diagnostics;
using NUnit.Framework;

namespace NLog.UnitTests
{
	[TestFixture]
	public class NLogTraceListenerTests : NLogTestBase
	{
		[Test]
		public void TraceWriteTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
				<nlog>
					<targets><target name='debug' type='Debug' layout='${logger} ${level} ${message}' /></targets>
					<rules>
						<logger name='*' minlevel='Debug' writeTo='debug' />
					</rules>
				</nlog>");

			Debug.Listeners.Clear();
			Debug.Listeners.Add(new NLogTraceListener { Name = "Logger1" });

			Debug.Write("Hello");
			AssertDebugLastMessage("debug", "Logger1 Debug Hello");

			Debug.Write("Hello", "Cat1");
			AssertDebugLastMessage("debug", "Logger1 Debug Cat1: Hello");

			using (ThCulture.Use())
			{
				Debug.Write(3.1415);
				AssertDebugLastMessage("debug", "Logger1 Debug 3.1415");

				Debug.Write(3.1415, "Cat2");
				AssertDebugLastMessage("debug", "Logger1 Debug Cat2: 3.1415");
			}
		}

		[Test]
		public void TraceWriteLineTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
				<nlog>
					<targets><target name='debug' type='Debug' layout='${logger} ${level} ${message}' /></targets>
					<rules>
						<logger name='*' minlevel='Debug' writeTo='debug' />
					</rules>
				</nlog>");

			Debug.Listeners.Clear();
			Debug.Listeners.Add(new NLogTraceListener { Name = "Logger1" });

			Debug.WriteLine("Hello");
			AssertDebugLastMessage("debug", "Logger1 Debug Hello");

			Debug.WriteLine("Hello", "Cat1");
			AssertDebugLastMessage("debug", "Logger1 Debug Cat1: Hello");

			using (ThCulture.Use())
			{
				Debug.WriteLine(3.1415);
				AssertDebugLastMessage("debug", "Logger1 Debug 3.1415");

				Debug.WriteLine(3.1415, "Cat2");
				AssertDebugLastMessage("debug", "Logger1 Debug Cat2: 3.1415");
			}
		}

		[Test]
		public void TraceWriteNonDefaultLevelTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
				<nlog>
					<targets><target name='debug' type='Debug' layout='${logger} ${level} ${message}' /></targets>
					<rules>
						<logger name='*' minlevel='Trace' writeTo='debug' />
					</rules>
				</nlog>");

			Debug.Listeners.Clear();
			Debug.Listeners.Add(new NLogTraceListener { Name = "Logger1", DefaultLogLevel = LogLevel.Trace });

			Debug.Write("Hello");
			AssertDebugLastMessage("debug", "Logger1 Trace Hello");
		}

		[Test]
		public void TraceConfiguration()
		{
			var listener = new NLogTraceListener();
			listener.Attributes.Add("defaultLogLevel", "Warn");
			listener.Attributes.Add("forceLogLevel", "Error");
			listener.Attributes.Add("autoLoggerName", "1");

			Assert.AreEqual(LogLevel.Warn, listener.DefaultLogLevel);
			Assert.AreEqual(LogLevel.Error, listener.ForceLogLevel);
			Assert.IsTrue(listener.AutoLoggerName);
		}

		[Test]
		public void TraceFailTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
				<nlog>
					<targets><target name='debug' type='Debug' layout='${logger} ${level} ${message}' /></targets>
					<rules>
						<logger name='*' minlevel='Debug' writeTo='debug' />
					</rules>
				</nlog>");

			Debug.Listeners.Clear();
			Debug.Listeners.Add(new NLogTraceListener { Name = "Logger1" });

			Debug.Fail("Message");
			AssertDebugLastMessage("debug", "Logger1 Error Message");

			Debug.Fail("Message", "Detailed Message");
			AssertDebugLastMessage("debug", "Logger1 Error Message Detailed Message");
		}

		[Test]
		public void AutoLoggerNameTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
				<nlog>
					<targets><target name='debug' type='Debug' layout='${logger} ${level} ${message}' /></targets>
					<rules>
						<logger name='*' minlevel='Debug' writeTo='debug' />
					</rules>
				</nlog>");

			Debug.Listeners.Clear();
			Debug.Listeners.Add(new NLogTraceListener { Name = "Logger1", AutoLoggerName = true });

			Debug.Write("Hello");
			AssertDebugLastMessage("debug", this.GetType().FullName + " Debug Hello");
		}

		[Test]
		public void TraceDataTests()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
				<nlog>
					<targets><target name='debug' type='Debug' layout='${logger} ${level} ${message} ${event-context:EventID}' /></targets>
					<rules>
						<logger name='*' minlevel='Trace' writeTo='debug' />
					</rules>
				</nlog>");

			TraceSource ts = CreateTraceSource();
			ts.Listeners.Add(new NLogTraceListener { Name = "Logger1", DefaultLogLevel = LogLevel.Trace });

			using (ThCulture.Use())
			{
				ts.TraceData(TraceEventType.Critical, 123, 42);
				AssertDebugLastMessage("debug", "MySource1 Fatal 42 123");

				ts.TraceData(TraceEventType.Critical, 145, 42, 3.14, "foo");
				AssertDebugLastMessage("debug", "MySource1 Fatal 42, 3.14, foo 145");
			}
		}
		
		[Test]
		public void LogInformationTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
				<nlog>
					<targets><target name='debug' type='Debug' layout='${logger} ${level} ${message} ${event-context:EventID}' /></targets>
					<rules>
						<logger name='*' minlevel='Trace' writeTo='debug' />
					</rules>
				</nlog>");

			TraceSource ts = CreateTraceSource();
			ts.Listeners.Add(new NLogTraceListener { Name = "Logger1", DefaultLogLevel = LogLevel.Trace });
			
			//ts.TraceInformation("Quick brown fox");
			ts.TraceEvent(TraceEventType.Information, 0, "Quick brown fox"); //HACK: not implemented
			AssertDebugLastMessage("debug", "MySource1 Info Quick brown fox 0");

			//ts.TraceInformation("Mary had {0} lamb", "a little");
			ts.TraceEvent(TraceEventType.Information, 0, "Mary had {0} lamb", "a little"); //HACK: not implemented
			AssertDebugLastMessage("debug", "MySource1 Info Mary had a little lamb 0");
		}

		[Test]
		public void TraceEventTests()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
				<nlog>
					<targets><target name='debug' type='Debug' layout='${logger} ${level} ${message} ${event-context:EventID}' /></targets>
					<rules>
						<logger name='*' minlevel='Trace' writeTo='debug' />
					</rules>
				</nlog>");

			TraceSource ts = CreateTraceSource();
			ts.Listeners.Add(new NLogTraceListener { Name = "Logger1", DefaultLogLevel = LogLevel.Trace });

			ts.TraceEvent(TraceEventType.Information, 123, "Quick brown {0} jumps over the lazy {1}.", "fox", "dog");
			AssertDebugLastMessage("debug", "MySource1 Info Quick brown fox jumps over the lazy dog. 123");

			ts.TraceEvent(TraceEventType.Information, 123);
			AssertDebugLastMessage("debug", "MySource1 Info  123");

			ts.TraceEvent(TraceEventType.Verbose, 145, "Bar");
			AssertDebugLastMessage("debug", "MySource1 Trace Bar 145");

			ts.TraceEvent(TraceEventType.Error, 145, "Foo");
			AssertDebugLastMessage("debug", "MySource1 Error Foo 145");

			ts.TraceEvent(TraceEventType.Suspend, 145, "Bar");
			AssertDebugLastMessage("debug", "MySource1 Debug Bar 145");

			ts.TraceEvent(TraceEventType.Resume, 145, "Foo");
			AssertDebugLastMessage("debug", "MySource1 Debug Foo 145");

			ts.TraceEvent(TraceEventType.Warning, 145, "Bar");
			AssertDebugLastMessage("debug", "MySource1 Warn Bar 145");

			ts.TraceEvent(TraceEventType.Critical, 145, "Foo");
			AssertDebugLastMessage("debug", "MySource1 Fatal Foo 145");
		}

		[Test]
		public void ForceLogLevelTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
				<nlog>
					<targets><target name='debug' type='Debug' layout='${logger} ${level} ${message} ${event-context:EventID}' /></targets>
					<rules>
						<logger name='*' minlevel='Trace' writeTo='debug' />
					</rules>
				</nlog>");

			TraceSource ts = CreateTraceSource();
			ts.Listeners.Add(new System.Diagnostics.ConsoleTraceListener());

			ts.Listeners.Add(new NLogTraceListener { Name = "Logger1", DefaultLogLevel = LogLevel.Trace, ForceLogLevel = LogLevel.Warn });

			// force all logs to be Warn, DefaultLogLevel has no effect on TraceSource
			//ts.TraceInformation("Quick brown fox");
			ts.TraceEvent(TraceEventType.Information, 0, "Quick brown fox"); //HACK: not implemented
			AssertDebugLastMessage("debug", "MySource1 Warn Quick brown fox 0");

			//ts.TraceInformation("Mary had {0} lamb", "a little");
			ts.TraceEvent(TraceEventType.Information, 0, "Mary had {0} lamb", "a little"); //HACK: not implemented
			AssertDebugLastMessage("debug", "MySource1 Warn Mary had a little lamb 0");
		}

		
		private static TraceSource CreateTraceSource()
		{
			var ts = new TraceSource("MySource1", SourceLevels.All);

			// for some reason needed on Mono
			ts.Switch = new SourceSwitch("MySource1", "Verbose");
			ts.Switch.Level = SourceLevels.All;

			return ts;
		}
	}
}
