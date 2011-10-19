
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

namespace NLog.UnitTests.Filters
{
	[TestFixture]
	public class WhenNotEqualTests : NLogTestBase
	{
		[Test]
		public void WhenNotEqualTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug'>
					<filters>
						<whenNotEqual layout='${message}' compareTo='skipme' action='Ignore' />
					</filters>
					</logger>
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			AssertDebugCounter("debug", 0);
			logger.Debug("skipme");
			AssertDebugCounter("debug", 1);
			logger.Debug("SkipMe");
			AssertDebugCounter("debug", 1);
		}

		[Test]
		public void WhenNotEqualInsensitiveTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug'>
					<filters>
						<whenNotEqual layout='${message}' compareTo='skipmetoo' action='Ignore' ignoreCase='true' />
					</filters>
					</logger>
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			AssertDebugCounter("debug", 0);
			logger.Debug("skipMeToo");
			AssertDebugCounter("debug", 1);
			logger.Debug("skipmetoo");
			AssertDebugCounter("debug", 2);
			logger.Debug("dontskipme");
			AssertDebugCounter("debug", 2);
		}
	}
}