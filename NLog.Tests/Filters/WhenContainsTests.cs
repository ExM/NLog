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
	public class WhenContainsTests : NLogTestBase
	{
		[Test]
		public void WhenContainsTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug'>
					<filters>
						<whenContains layout='${message}' substring='zzz' action='Ignore' />
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
			AssertDebugCounter("debug", 2);
		}

		[Test]
		public void WhenContainsInsensitiveTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug'>
					<filters>
						<whenContains layout='${message}' substring='zzz' action='Ignore' ignoreCase='true' />
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
			logger.Debug("aaa");
			AssertDebugCounter("debug", 2);
		}
	}
}