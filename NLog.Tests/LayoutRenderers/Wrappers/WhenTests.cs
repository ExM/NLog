using System;
using NLog;
using NLog.Layouts;
using NUnit.Framework;
using NLog.Common;

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
	[TestFixture]
	public class WhenTests : NLogTestBase
	{
		[Test]
		public void PositiveWhenTest()
		{
			SimpleLayout l = @"${message:when=logger=='logger'}";
			l.Initialize(CommonCfg);
			var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
			Assert.AreEqual("message", l.Render(le));
		}

		[Test]
		public void NegativeWhenTest()
		{
			SimpleLayout l = @"${message:when=logger=='logger'}";
			l.Initialize(CommonCfg);
			var le = LogEventInfo.Create(LogLevel.Info, "logger2", "message");
			Assert.AreEqual("", l.Render(le));
		}

		[Test]
		public void ComplexWhenTest()
		{
			// condition is pretty complex here and includes nested layout renderers
			// we are testing here that layout parsers property invokes Condition parser to consume the right number of characters
			SimpleLayout l = @"${message:when='${pad:${logger}:padding=10:padCharacter=X}'=='XXXXlogger':padding=-10:padCharacter=Y}";
			l.Initialize(CommonCfg);
			var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
			Assert.AreEqual("messageYYY", l.Render(le));
		}

		[Test]
		public void ComplexWhenTest2()
		{
			// condition is pretty complex here and includes nested layout renderers
			// we are testing here that layout parsers property invokes Condition parser to consume the right number of characters
			SimpleLayout l = @"${message:padding=-10:padCharacter=Y:when='${pad:${logger}:padding=10:padCharacter=X}'=='XXXXlogger'}";
			l.Initialize(CommonCfg);
			var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
			Assert.AreEqual("messageYYY", l.Render(le));
		}
	}
}