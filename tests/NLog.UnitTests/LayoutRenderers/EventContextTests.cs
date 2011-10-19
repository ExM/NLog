
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
using NLog.Internal;
using NLog.Layouts;

namespace NLog.UnitTests.LayoutRenderers
{
	[TestFixture]
	public class EventContextTests : NLogTestBase
	{
		[Test]
		public void Test1()
		{
			Layout l = "${event-context:aaa}";
			LogEventInfo lei = LogEventInfo.Create(LogLevel.Info, "aaa", "bbb");

			// empty
			Assert.AreEqual("", l.Render(lei));
		}

		[Test]
		public void Test2()
		{
			Layout l = "${event-context:aaa}";
			LogEventInfo lei = LogEventInfo.Create(LogLevel.Info, "aaa", "bbb");
			lei.Properties["aaa"] = "bbb";

			// empty
			Assert.AreEqual("bbb", l.Render(lei));
		}
	}
}
