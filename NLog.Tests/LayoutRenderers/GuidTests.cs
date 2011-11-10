using NLog;
using NUnit.Framework;
using NLog.Internal;
using NLog.Layouts;

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
	[TestFixture]
	public class GuidTests : NLogTestBase
	{
		[Test]
		public void Render()
		{
			SimpleLayout l = "${guid}";
			l.Initialize(CommonCfg);
			string s1 = l.Render(LogEventInfo.CreateNullEvent());
			string s2 = l.Render(LogEventInfo.CreateNullEvent());

			Assert.AreNotEqual(s1, s2);
		}
	}
}