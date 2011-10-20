using System;
using System.Xml;
using System.Reflection;
using System.IO;
using NLog;
using NLog.Config;
using NUnit.Framework;

namespace NLog.UnitTests.LayoutRenderers
{
	[TestFixture]
	public class LiteralTests : NLogTestBase
	{
		[Test]
		public void LiteralTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='abcd' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			AssertDebugLastMessage("debug", "abcd");
		}
	}
}
