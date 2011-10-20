
namespace NLog.UnitTests.Config
{
	using System;
	using NUnit.Framework;

#if !NUNIT
	using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
	using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
	using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
	using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
	using NLog.Common;

	[TestFixture]
	public class InternalLoggingTests : NLogTestBase
	{
		[Test]
		public void InternalLoggingConfigTest1()
		{
			using (var scope = new InternalLoggerScope())
			{
				CreateConfigurationFromString(@"
<nlog internalLogFile='c:\file.txt' internalLogLevel='Trace' internalLogToConsole='true' internalLogToConsoleError='true' globalThreshold='Warn' throwExceptions='true'>
</nlog>");

				Assert.AreSame(LogLevel.Trace, InternalLogger.LogLevel);
				Assert.IsTrue(InternalLogger.LogToConsole);
#if !NET_CF
				Assert.IsTrue(InternalLogger.LogToConsoleError);
#endif
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
#if !NET_CF
				InternalLogger.LogToConsoleError = true;
#endif
				LogManager.GlobalThreshold = LogLevel.Fatal;
				LogManager.ThrowExceptions = true;

				CreateConfigurationFromString(@"
<nlog>
</nlog>");

				Assert.AreSame(LogLevel.Error, InternalLogger.LogLevel);
				Assert.IsTrue(InternalLogger.LogToConsole);
#if !NET_CF
				Assert.IsTrue(InternalLogger.LogToConsoleError);
#endif
				Assert.AreSame(LogLevel.Fatal, LogManager.GlobalThreshold);
				Assert.IsTrue(LogManager.ThrowExceptions);
			}
		}
	}
}