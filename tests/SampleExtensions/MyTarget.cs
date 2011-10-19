
namespace MyExtensionNamespace
{
	using NLog;
	using NLog.Targets;

	[Target("MyTarget")]
	public class MyTarget : Target
	{
		protected override void Write(LogEventInfo logEvent)
		{
			// do nothing
		}
	}
}