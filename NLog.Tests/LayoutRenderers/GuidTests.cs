using NLog;
using NUnit.Framework;
using NLog.Internal;
using NLog.Layouts;
using NLog.Common;

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
	[TestFixture]
	public class GuidTests : NLogTestBase
	{
		[Test]
		public void Render()
		{
			SimpleLayout l = "${guid}";
			l.DeepInitialize(CommonCfg);
			string s1 = l.Render(LogEventInfo.CreateNullEvent());
			string s2 = l.Render(LogEventInfo.CreateNullEvent());

			Assert.AreNotEqual(s1, s2);
		}
	}
}