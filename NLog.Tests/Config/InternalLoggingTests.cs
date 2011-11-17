using System;
using NUnit.Framework;
using NLog.Common;

namespace NLog.UnitTests.Config
{
	[TestFixture]
	public class InternalLoggingTests : NLogTestBase
	{
		[Test]
		public void InternalLoggingConfigTest1()
		{
			using (var scope = new InternalLoggerScope())
			{
				CreateConfigurationFromString(@"
<nlog internalLogFile='internalLog.txt' internalLogLevel='Trace' internalLogToConsole='true' internalLogToConsoleError='true' globalThreshold='Warn' throwExceptions='true'>
</nlog>");
				Assert.AreSame(LogLevel.Trace, InternalLogger.LogLevel);
				Assert.IsTrue(InternalLogger.LogToConsole);
				Assert.IsTrue(InternalLogger.LogToConsoleError);
				Assert.AreSame(LogLevel.Warn, LogManager.GlobalThreshold);
				Assert.IsTrue(LogManager.ThrowExceptions);
			}
		}

		[Test]
		public void InternalLoggingConfigTest2()
		{
			using (new InternalLoggerScope())
			{
				InternalLogger.LogLevel = LogLevel.Error;
				InternalLogger.LogToConsole = true;
				InternalLogger.LogToConsoleError = true;
				LogManager.GlobalThreshold = LogLevel.Fatal;
				LogManager.ThrowExceptions = true;

				CreateConfigurationFromString(@"
<nlog>
</nlog>");

				Assert.AreSame(LogLevel.Error, InternalLogger.LogLevel);
				Assert.IsTrue(InternalLogger.LogToConsole);
				Assert.IsTrue(InternalLogger.LogToConsoleError);
				Assert.AreSame(LogLevel.Fatal, LogManager.GlobalThreshold);
				Assert.IsTrue(LogManager.ThrowExceptions);
			}
		}
	}
}