using System;
using System.IO;
using NUnit.Framework;
using NLog.Targets;
using System.Collections.Generic;
using NLog.Common;

namespace NLog.UnitTests.Targets
{
	[TestFixture]
	public class ConsoleTargetTests : NLogTestBase
	{
		[Test]
		public void ConsoleOutTest()
		{
			var target = new ConsoleTarget()
			{
				Header = "-- header --",
				Layout = "${logger} ${message}",
				Footer = "-- footer --",
			};

			var consoleOutWriter = new StringWriter();
			TextWriter oldConsoleOutWriter = Console.Out;
			Console.SetOut(consoleOutWriter);

			try
			{
				var exceptions = new List<Exception>();
				using (target.Initialize(CommonCfg))
				{
					target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "Logger1", "message1").WithContinuation(exceptions.Add));
					target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "Logger1", "message2").WithContinuation(exceptions.Add));
					target.WriteAsyncLogEvents(
						new LogEventInfo(LogLevel.Info, "Logger1", "message3").WithContinuation(exceptions.Add),
						new LogEventInfo(LogLevel.Info, "Logger2", "message4").WithContinuation(exceptions.Add),
						new LogEventInfo(LogLevel.Info, "Logger2", "message5").WithContinuation(exceptions.Add),
						new LogEventInfo(LogLevel.Info, "Logger1", "message6").WithContinuation(exceptions.Add));
					Assert.AreEqual(6, exceptions.Count);
				}
			}
			finally 
			{
				Console.SetOut(oldConsoleOutWriter);
			}

			string expectedResult = @"-- header --
Logger1 message1
Logger1 message2
Logger1 message3
Logger2 message4
Logger2 message5
Logger1 message6
-- footer --
";
			Assert.AreEqual(expectedResult, consoleOutWriter.ToString());
		}

		[Test]
		public void ConsoleErrorTest()
		{
			var target = new ConsoleTarget()
			{
				Header = "-- header --",
				Layout = "${logger} ${message}",
				Footer = "-- footer --",
				Error = true,
			};

			var consoleErrorWriter = new StringWriter();
			TextWriter oldConsoleErrorWriter = Console.Error;
			Console.SetError(consoleErrorWriter);

			try
			{
				var exceptions = new List<Exception>();
				using(target.Initialize(CommonCfg))
				{
					target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "Logger1", "message1").WithContinuation(exceptions.Add));
					target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "Logger1", "message2").WithContinuation(exceptions.Add));
					target.WriteAsyncLogEvents(
						new LogEventInfo(LogLevel.Info, "Logger1", "message3").WithContinuation(exceptions.Add),
						new LogEventInfo(LogLevel.Info, "Logger2", "message4").WithContinuation(exceptions.Add),
						new LogEventInfo(LogLevel.Info, "Logger2", "message5").WithContinuation(exceptions.Add),
						new LogEventInfo(LogLevel.Info, "Logger1", "message6").WithContinuation(exceptions.Add));
					Assert.AreEqual(6, exceptions.Count);
				}
			}
			finally
			{
				Console.SetError(oldConsoleErrorWriter);
			}

			string expectedResult = @"-- header --
Logger1 message1
Logger1 message2
Logger1 message3
Logger2 message4
Logger2 message5
Logger1 message6
-- footer --
";
			Assert.AreEqual(expectedResult, consoleErrorWriter.ToString());
		}
	}
}
