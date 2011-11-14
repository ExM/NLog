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

	[TestFixture]
	public class LongDateTests : NLogTestBase
	{
		[Test]
		public void LongDateTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${longdate}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			LogManager.GetLogger("d").Debug("zzz");
			string date = GetDebugLastMessage("debug");
			Assert.AreEqual(date.Length, 24);
			Assert.AreEqual(date[4], '-');
			Assert.AreEqual(date[7], '-');
			Assert.AreEqual(date[10], ' ');
			Assert.AreEqual(date[13], ':');
			Assert.AreEqual(date[16], ':');
			Assert.AreEqual(date[19], '.');
		}

		[Test]
		public void UniversalTimeTest()
		{
			var dt = new LongDateLayoutRenderer();
			dt.UniversalTime = true;
			
			var ei = new LogEventInfo(LogLevel.Info, "logger", "msg");
			dt.DeepInitialize(CommonCfg);
			Assert.AreEqual(ei.TimeStamp.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.ffff"), dt.Render(ei));
		}

		[Test]
		public void LocalTimeTest()
		{
			var dt = new LongDateLayoutRenderer();
			dt.UniversalTime = false;
			
			var ei = new LogEventInfo(LogLevel.Info, "logger", "msg");
			dt.DeepInitialize(CommonCfg);
			Assert.AreEqual(ei.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss.ffff"), dt.Render(ei));
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