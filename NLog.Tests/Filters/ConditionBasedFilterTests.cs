
namespace NLog.UnitTests.Filters
{
	using System;
	using System.Xml;
	using System.Reflection;
	using System.IO;

	using NLog;
	using NLog.Config;

	using NUnit.Framework;

#if !NUNIT
	using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
	using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
	using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
	using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

	[TestFixture]
	public class ConditionBasedFilterTests : NLogTestBase
	{
		[Test]
		public void WhenTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug'>
					<filters>
						<when condition=""contains(message,'zzz')"" action='Ignore' />
					</filters>
					</logger>
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			AssertDebugCounter("debug", 1);
			logger.Debug("zzz");
			AssertDebugCounter("debug", 1);
			logger.Debug("ZzzZ");
			AssertDebugCounter("debug", 1);
			logger.Debug("Zz");
			AssertDebugCounter("debug", 2);
		}
	}
}