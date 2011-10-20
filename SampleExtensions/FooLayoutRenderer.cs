using System.Text;
using NLog;
using NLog.LayoutRenderers;
	
namespace MyExtensionNamespace
{
	[LayoutRenderer("foo")]
	public class FooLayoutRenderer : LayoutRenderer
	{
		protected override void Append(StringBuilder buffer, LogEventInfo logEvent)
		{
			buffer.Append("foo");
		}
	}
}