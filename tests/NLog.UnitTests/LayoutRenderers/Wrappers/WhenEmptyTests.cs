
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
	public class WhenEmptyTests : NLogTestBase
	{
		[Test]
		public void CoalesceTest()
		{
			SimpleLayout l = @"${message:whenEmpty=<no message>}";

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

			var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
			Assert.AreEqual("message", l.Render(le));

			// empty log message
			var le2 = LogEventInfo.Create(LogLevel.Info, "mylogger", "");
			Assert.AreEqual("mylogger emitted empty message", l.Render(le2));
		}
	}
}