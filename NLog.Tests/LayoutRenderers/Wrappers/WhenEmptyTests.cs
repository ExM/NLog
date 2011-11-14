using System;
using NLog;
using NUnit.Framework;
using NLog.Internal;
using NLog.Layouts;
using NLog.Common;

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
	[TestFixture]
	public class WhenEmptyTests : NLogTestBase
	{
		[Test]
		public void CoalesceTest()
		{
			SimpleLayout l = @"${message:whenEmpty=<no message>}";
			l.DeepInitialize(CommonCfg);
			var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
			Assert.AreEqual("message", l.Render(le));

			// empty log message
			var le2 = LogEventInfo.Create(LogLevel.Info, "logger", "");
			Assert.AreEqual("<no message>", l.Render(le2));
		}

		[Test]
		public void CoalesceWithANestedLayout()
		{
			SimpleLayout l = @"${message:whenEmpty=${logger} emitted empty message}";
			l.DeepInitialize(CommonCfg);
			var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
			Assert.AreEqual("message", l.Render(le));

			// empty log message
			var le2 = LogEventInfo.Create(LogLevel.Info, "mylogger", "");
			Assert.AreEqual("mylogger emitted empty message", l.Render(le2));
		}
	}
}