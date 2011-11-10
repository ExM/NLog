using System;
using System.Xml;
using System.Reflection;
using NLog;
using NLog.Config;
using NUnit.Framework;

namespace NLog.UnitTests.LayoutRenderers
{
	using NLog.LayoutRenderers;

	[TestFixture]
	public class TimeTests : NLogTestBase
	{
		[Test]
		public void UniversalTimeTest()
		{
			var dt = new TimeLayoutRenderer();
			dt.UniversalTime = true;

			var ei = new LogEventInfo(LogLevel.Info, "logger", "msg");
			dt.Initialize(CommonCfg);
			Assert.AreEqual(ei.TimeStamp.ToUniversalTime().ToString("HH:mm:ss.ffff"), dt.Render(ei));
		}

		[Test]
		public void LocalTimeTest()
		{
			var dt = new TimeLayoutRenderer();
			dt.UniversalTime = false;

			var ei = new LogEventInfo(LogLevel.Info, "logger", "msg");
			dt.Initialize(CommonCfg);
			Assert.AreEqual(ei.TimeStamp.ToString("HH:mm:ss.ffff"), dt.Render(ei));
		}
		
		[Test]
		public void TimeTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${time}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			LogManager.GetLogger("d").Debug("zzz");
			string date = GetDebugLastMessage("debug");
			Assert.AreEqual(date.Length, 13);
			Assert.AreEqual(date[2], ':');
			Assert.AreEqual(date[5], ':');
			Assert.AreEqual(date[8], '.');
		}

		[Test]
		public void LongDateWithPadding()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${longdate:padding=5:fixedlength=true}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			LogManager.GetLogger("d").Debug("zzz");
			string date = GetDebugLastMessage("debug");
			Assert.AreEqual(5, date.Length);
			Assert.AreEqual(date[4], '-');
		}
	}
}