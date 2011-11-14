using NLog;
using NUnit.Framework;
using NLog.Layouts;
using NLog.Common;

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
	[TestFixture]
	public class JsonEncodeTests : NLogTestBase
	{
		[Test]
		public void JsonEncodeTest1()
		{
			MappedDiagnosticsContext.Clear();
			MappedDiagnosticsContext.Set("foo", " abc\"\n\b\r\f\t/\u1234\u5432\\xyz ");
			SimpleLayout l = "${json-encode:${mdc:foo}}";
			l.DeepInitialize(CommonCfg);
			Assert.AreEqual(@" abc\""\n\b\r\f\t\/\u1234\u5432\\xyz ", l.Render(LogEventInfo.CreateNullEvent()));
		}
	}
}