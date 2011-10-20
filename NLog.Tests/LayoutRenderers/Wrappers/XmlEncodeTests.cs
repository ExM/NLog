
namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
	using NLog;
	using NUnit.Framework;
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