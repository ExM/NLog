
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
	using NLog.LayoutRenderers;

	[TestFixture]
	public class ShortDateTests : NLogTestBase
	{
		[Test]
		public void UniversalTimeTest()
		{
			var dt = new ShortDateLayoutRenderer();
			dt.UniversalTime = true;

			var ei = new LogEventInfo(LogLevel.Info, "logger", "msg");
			Assert.AreEqual(ei.TimeStamp.ToUniversalTime().ToString("yyyy-MM-dd"), dt.Render(ei));
		}

		[Test]
		public void LocalTimeTest()
		{
			var dt = new ShortDateLayoutRenderer();
			dt.UniversalTime = false;

			var ei = new LogEventInfo(LogLevel.Info, "logger", "msg");
			Assert.AreEqual(ei.TimeStamp.ToString("yyyy-MM-dd"), dt.Render(ei));
		}
		
		[Test]
		public void ShortDateTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${shortdate}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			LogManager.GetLogger("d").Debug("zzz");
			AssertDebugLastMessage("debug", DateTime.Now.ToString("yyyy-MM-dd"));
		}
	}
}