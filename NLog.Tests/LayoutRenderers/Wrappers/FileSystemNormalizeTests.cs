using NLog;
using NUnit.Framework;
using NLog.Layouts;
using NLog.Common;

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
	[TestFixture]
	public class FileSystemNormalizeTests : NLogTestBase
	{
		[Test]
		public void FSNormalizeTest1()
		{
			MappedDiagnosticsContext.Clear();
			MappedDiagnosticsContext.Set("foo", "abc.log");
			SimpleLayout l = "${filesystem-normalize:${mdc:foo}}";
			l.DeepInitialize(CommonCfg);
			Assert.AreEqual("abc.log", l.Render(LogEventInfo.CreateNullEvent()));

			MappedDiagnosticsContext.Set("foo", "");
			Assert.AreEqual("", l.Render(LogEventInfo.CreateNullEvent()));

			MappedDiagnosticsContext.Set("foo", "a/b/c");
			Assert.AreEqual("a_b_c", l.Render(LogEventInfo.CreateNullEvent()));

			// all characters outside of alpha/digits/space/_/./- are replaced with _
			MappedDiagnosticsContext.Set("foo", ":\\/$@#$%^");
			Assert.AreEqual("_________", l.Render(LogEventInfo.CreateNullEvent()));
		}

		[Test]
		public void FSNormalizeTest2()
		{
			MappedDiagnosticsContext.Clear();
			MappedDiagnosticsContext.Set("foo", "abc.log");
			SimpleLayout l = "${mdc:foo:fsnormalize=true}";
			l.DeepInitialize(CommonCfg);
			Assert.AreEqual("abc.log", l.Render(LogEventInfo.CreateNullEvent()));

			MappedDiagnosticsContext.Set("foo", "");
			Assert.AreEqual("", l.Render(LogEventInfo.CreateNullEvent()));

			MappedDiagnosticsContext.Set("foo", "a/b/c");
			Assert.AreEqual("a_b_c", l.Render(LogEventInfo.CreateNullEvent()));

			// all characters outside of alpha/digits/space/_/./- are replaced with _
			MappedDiagnosticsContext.Set("foo", ":\\/$@#$%^");
			Assert.AreEqual("_________", l.Render(LogEventInfo.CreateNullEvent()));
		}
	}
}