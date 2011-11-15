using NLog;
using NUnit.Framework;
using NLog.Internal;
using NLog.Layouts;
using NLog.Common;

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
	[TestFixture]
	public class CachedTests : NLogTestBase
	{
		[Test]
		public void ReInit()
		{
			string s1;
			string s2;
			string s3;
			// but when you apply ${cached}, the guid will only be generated once
			SimpleLayout l = "${cached:${guid}:cached=true}";
			using(l.Initialize(CommonCfg))
			{
				s1 = l.Render(LogEventInfo.CreateNullEvent());
				s2 = l.Render(LogEventInfo.CreateNullEvent());
				Assert.IsNotEmpty(s1);
				Assert.AreEqual(s1, s2);
			// calling Close() on Layout Renderer will reset the cached value
			}
			using(l.Initialize(CommonCfg))
			{
				s3 = l.Render(LogEventInfo.CreateNullEvent());
				Assert.IsNotEmpty(s3);
				Assert.AreNotEqual(s2, s3);
			// calling Initialize() on Layout Renderer will reset the cached value
			}
			l.Initialize(CommonCfg);
			string s4 = l.Render(LogEventInfo.CreateNullEvent());
			Assert.IsNotEmpty(s4);
			Assert.AreNotEqual(s3, s4);
		}

		[Test]
		public void CachedOff()
		{
			// another way to achieve the same thing is using cached=true
			SimpleLayout l = "${guid:cached=true}";
			l.Initialize(CommonCfg);
			string s1 = l.Render(LogEventInfo.CreateNullEvent());
			string s2 = l.Render(LogEventInfo.CreateNullEvent());
			Assert.AreEqual(s1, s2);
		}

		[Test]
		public void CachedOn()
		{
			// another way to achieve the same thing is using cached=true
			SimpleLayout l = "${guid:cached=false}";
			l.Initialize(CommonCfg);
			string s1 = l.Render(LogEventInfo.CreateNullEvent());
			string s2 = l.Render(LogEventInfo.CreateNullEvent());
			Assert.AreNotEqual(s1, s2);
		}
	}
}