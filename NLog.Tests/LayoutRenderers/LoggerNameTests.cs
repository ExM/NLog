
using System;
using System.Xml;
using System.Reflection;

using NLog;
using NLog.Config;

using NUnit.Framework;

#if !NUNIT
	using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
	using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
	using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
	using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

namespace NLog.UnitTests.LayoutRenderers
{
	[TestFixture]
	public class LoggerNameTests : NLogTestBase
	{
		[Test]
		public void LoggerNameTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${logger} ${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			AssertDebugLastMessage("debug", "A a");
		}
	}
}