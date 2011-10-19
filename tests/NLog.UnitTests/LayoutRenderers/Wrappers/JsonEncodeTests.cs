
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
	using NLog.Layouts;

	[TestFixture]
	public class JsonEncodeTests : NLogTestBase
	{
		[Test]
		public void JsonEncodeTest1()
		{
			MappedDiagnosticsContext.Clear();
			MappedDiagnosticsContext.Set("foo", " abc\"\n\b\r\f\t/\u1234\u5432\\xyz ");
			SimpleLayout l = "${json-encode:${mdc:foo}}";

			Assert.AreEqual(@" abc\""\n\b\r\f\t\/\u1234\u5432\\xyz ", l.Render(LogEventInfo.CreateNullEvent()));
		}
	}
}