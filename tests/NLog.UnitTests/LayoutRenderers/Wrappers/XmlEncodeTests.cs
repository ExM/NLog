
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
	public class XmlEncodeTests : NLogTestBase
	{
		[Test]
		public void XmlEncodeTest1()
		{
			MappedDiagnosticsContext.Clear();
			MappedDiagnosticsContext.Set("foo", " abc<>&'\"def ");
			SimpleLayout l = "${xml-encode:${mdc:foo}}";

			Assert.AreEqual(" abc&lt;&gt;&amp;&apos;&quot;def ", l.Render(LogEventInfo.CreateNullEvent()));
		}
	}
}