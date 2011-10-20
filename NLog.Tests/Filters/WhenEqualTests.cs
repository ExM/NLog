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
	public class WhenEqualTests : NLogTestBase
	{
		[Test]
		public void WhenEqualTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug'>
					<filters>
						<whenEqual layout='${message}' compareTo='skipme' action='Ignore' />
					</filters>
					</logger>
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			AssertDebugCounter("debug", 1);
			logger.Debug("skipme");
			AssertDebugCounter("debug", 1);
			logger.Debug("SkipMe");
			AssertDebugCounter("debug", 2);
		}

		[Test]
		public void WhenEqualInsensitiveTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug'>
					<filters>
						<whenEqual layout='${message}' compareTo='skipmetoo' action='Ignore' ignoreCase='true' />
					</filters>
					</logger>
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			AssertDebugCounter("debug", 1);
			logger.Debug("skipMeToo");
			AssertDebugCounter("debug", 1);
			logger.Debug("skipmetoo");
			AssertDebugCounter("debug", 1);
		}
	}
}