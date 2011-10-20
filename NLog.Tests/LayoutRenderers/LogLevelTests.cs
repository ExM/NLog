
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

namespace NLog.UnitTests.LayoutRenderers
{
	[TestFixture]
	public class LogLevelTests : NLogTestBase
	{
		[Test]
		public void LogLevelTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${level} ${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			AssertDebugLastMessage("debug", "Debug a");
			logger.Info("a");
			AssertDebugLastMessage("debug", "Info a");
			logger.Warn("a");
			AssertDebugLastMessage("debug", "Warn a");
			logger.Error("a");
			AssertDebugLastMessage("debug", "Error a");
			logger.Fatal("a");
			AssertDebugLastMessage("debug", "Fatal a");
		}
	}
}