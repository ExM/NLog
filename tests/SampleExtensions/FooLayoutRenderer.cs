
namespace MyExtensionNamespace
{
	using System.Text;

	using NLog;
	using NLog.LayoutRenderers;

	[LayoutRenderer("foo")]
	public class FooLayoutRenderer : LayoutRenderer
	{
		protected override void Append(StringBuilder buffer, LogEventInfo logEvent)
		{
			buffer.Append("foo");
		}
	}
}