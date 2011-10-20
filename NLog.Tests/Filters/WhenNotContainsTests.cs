using System;
using System.Xml;
using System.Reflection;
using System.IO;
using NLog;
using NLog.Config;
using NUnit.Framework;

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