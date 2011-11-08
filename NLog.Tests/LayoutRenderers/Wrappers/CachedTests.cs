
namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
	using NLog;
	using NUnit.Framework;
	using NLog.Internal;
	using NLog.Layouts;

	[TestFixture]
	public class CachedTests : NLogTestBase
	{
		[Test]
		public void CachedLayoutRendererWrapper()
		{
			SimpleLayout l = "${guid}";
			l.Initialize(CommonCfg);
			string s1 = l.Render(LogEventInfo.CreateNullEvent());
			string s2 = l.Render(LogEventInfo.CreateNullEvent());
			string s3;

			// normally GUIDs are never the same
			Assert.AreNotEqual(s1, s2);

			// but when you apply ${cached}, the guid will only be generated once
			l = "${cached:${guid}:cached=true}";
			l.Initialize(CommonCfg);
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
			l.Initialize(CommonCfg);
			s1 = l.Render(LogEventInfo.CreateNullEvent());
			s2 = l.Render(LogEventInfo.CreateNullEvent());
			Assert.AreEqual(s1, s2);

			// another way to achieve the same thing is using cached=true
			l = "${guid:cached=false}";
			l.Initialize(CommonCfg);
			s1 = l.Render(LogEventInfo.CreateNullEvent());
			s2 = l.Render(LogEventInfo.CreateNullEvent());
			Assert.AreNotEqual(s1, s2);
		}
	}
}