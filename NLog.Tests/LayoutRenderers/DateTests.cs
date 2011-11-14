using System;
using System.Xml;
using System.Reflection;
using NLog;
using NLog.Config;
using NUnit.Framework;
using NLog.Common;

namespace NLog.UnitTests.LayoutRenderers
{
	using NLog.LayoutRenderers;
	using System.Globalization;

	[TestFixture]
	public class DateTests : NLogTestBase
	{
		[Test]
		public void DefaultDateTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${date}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			LogManager.GetLogger("d").Debug("zzz");
			DateTime dt = DateTime.Parse(GetDebugLastMessage("debug"), CultureInfo.InvariantCulture);
			DateTime now = DateTime.Now;

			Assert.IsTrue(Math.Abs((dt - now).TotalSeconds) < 5);
		}

		[Test]
		public void UniversalTimeTest()
		{
			var dt = new DateLayoutRenderer();
			dt.UniversalTime = true;
			dt.Format = "R";

			var ei = new LogEventInfo(LogLevel.Info, "logger", "msg");
			dt.DeepInitialize(CommonCfg);
			Assert.AreEqual(ei.TimeStamp.ToUniversalTime().ToString("R"), dt.Render(ei));
		}

		[Test]
		public void LocalTimeTest()
		{
			var dt = new DateLayoutRenderer();
			dt.UniversalTime = false;
			dt.Format = "R";

			var ei = new LogEventInfo(LogLevel.Info, "logger", "msg");
			dt.DeepInitialize(CommonCfg);
			Assert.AreEqual(ei.TimeStamp.ToString("R"), dt.Render(ei));
		}

		[Test]
		public void FormattedDateTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${date:format=yyyy-MM-dd}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			LogManager.GetLogger("d").Debug("zzz");
			AssertDebugLastMessage("debug", DateTime.Now.ToString("yyyy-MM-dd"));
		}
	}
}