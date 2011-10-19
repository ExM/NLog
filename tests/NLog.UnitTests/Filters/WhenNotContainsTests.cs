
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
	public class WhenNotContainsTests : NLogTestBase
	{
		[Test]
		public void WhenNotContainsTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug'>
					<filters>
						<whenNotContains layout='${message}' substring='zzz' action='Ignore' />
					</filters>
					</logger>
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			AssertDebugCounter("debug", 0);
			logger.Debug("zzz");
			AssertDebugCounter("debug", 1);
			logger.Debug("ZzzZ");
			AssertDebugCounter("debug", 1);
		}

		[Test]
		public void WhenNotContainsInsensitiveTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug'>
					<filters>
						<whenNotContains layout='${message}' substring='zzz' action='Ignore' ignoreCase='true' />
					</filters>
					</logger>
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			AssertDebugCounter("debug", 0);
			logger.Debug("zzz");
			AssertDebugCounter("debug", 1);
			logger.Debug("ZzzZ");
			AssertDebugCounter("debug", 2);
			logger.Debug("aaa");
			AssertDebugCounter("debug", 2);
		}
	}
}