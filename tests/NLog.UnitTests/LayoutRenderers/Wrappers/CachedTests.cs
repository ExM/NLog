
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
	public class CachedTests : NLogTestBase
	{
		[Test]
		public void CachedLayoutRendererWrapper()
		{
			SimpleLayout l = "${guid}";
			
			string s1 = l.Render(LogEventInfo.CreateNullEvent());
			string s2 = l.Render(LogEventInfo.CreateNullEvent());
			string s3;

			// normally GUIDs are never the same
			Assert.AreNotEqual(s1, s2);

			// but when you apply ${cached}, the guid will only be generated once
			l = "${cached:${guid}:cached=true}";
			s1 = l.Render(LogEventInfo.CreateNullEvent());
			s2 = l.Render(LogEventInfo.CreateNullEvent());
			Assert.AreEqual(s1, s2);

			// calling Close() on Layout Renderer will reset the cached value
			l.Renderers[0].Close();
			s3 = l.Render(LogEventInfo.CreateNullEvent());
			Assert.AreNotEqual(s2, s3);

			// calling Initialize() on Layout Renderer will reset the cached value
			l.Renderers[0].Close();
			string s4 = l.Render(LogEventInfo.CreateNullEvent());
			Assert.AreNotEqual(s3, s4);

			// another way to achieve the same thing is using cached=true
			l = "${guid:cached=true}";
			s1 = l.Render(LogEventInfo.CreateNullEvent());
			s2 = l.Render(LogEventInfo.CreateNullEvent());
			Assert.AreEqual(s1, s2);

			// another way to achieve the same thing is using cached=true
			l = "${guid:cached=false}";
			s1 = l.Render(LogEventInfo.CreateNullEvent());
			s2 = l.Render(LogEventInfo.CreateNullEvent());
			Assert.AreNotEqual(s1, s2);
		}
	}
}