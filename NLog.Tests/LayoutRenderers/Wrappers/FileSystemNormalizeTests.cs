
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
	public class FileSystemNormalizeTests : NLogTestBase
	{
		[Test]
		public void FSNormalizeTest1()
		{
			MappedDiagnosticsContext.Clear();
			MappedDiagnosticsContext.Set("foo", "abc.log");
			SimpleLayout l = "${filesystem-normalize:${mdc:foo}}";
			
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