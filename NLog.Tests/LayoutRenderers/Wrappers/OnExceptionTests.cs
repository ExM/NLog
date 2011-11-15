using System;
using NLog;
using NUnit.Framework;
using NLog.Internal;
using NLog.Layouts;
using NLog.Common;

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
	[TestFixture]
	public class OnExceptionTests : NLogTestBase
	{
		[Test]
		public void OnExceptionTest1()
		{
			SimpleLayout l = @"${message}${onexception:EXCEPTION\:${exception:format=message}}${logger}";
			l.Initialize(CommonCfg);
			// no exception - ${onexception} is ignored completely
			var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
			Assert.AreEqual("messagelogger", l.Render(le));

			// have exception
			var le2 = LogEventInfo.Create(LogLevel.Info, "logger", "message");
			le2.Exception = new InvalidOperationException("ExceptionMessage");
			Assert.AreEqual("messageEXCEPTION:ExceptionMessagelogger", l.Render(le2));
		}
	}
}