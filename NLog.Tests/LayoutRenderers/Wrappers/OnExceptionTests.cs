
namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
	using System;
	using NLog;
	using NUnit.Framework;

#if !NUNIT
	using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
	using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
	using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
	using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
	using NLog.Internal;
	using NLog.Layouts;

	[TestFixture]
	public class OnExceptionTests : NLogTestBase
	{
		[Test]
		public void OnExceptionTest1()
		{
			SimpleLayout l = @"${message}${onexception:EXCEPTION\:${exception:format=message}}${logger}";

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