
namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
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
	public class TrimWhiteSpaceTests : NLogTestBase
	{
		[Test]
		public void TrimWhiteSpaceTest1()
		{
			MappedDiagnosticsContext.Clear();
			MappedDiagnosticsContext.Set("foo", "  bar  ");
			SimpleLayout l = "${trim-whitespace:${mdc:foo}}";
			
			Assert.AreEqual("bar", l.Render(LogEventInfo.CreateNullEvent()));

			MappedDiagnosticsContext.Set("foo", "");
			Assert.AreEqual("", l.Render(LogEventInfo.CreateNullEvent()));

			MappedDiagnosticsContext.Set("foo", "	");
			Assert.AreEqual("", l.Render(LogEventInfo.CreateNullEvent()));
		}
	}
}