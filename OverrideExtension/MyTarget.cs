using NLog;
using NLog.Targets;
	
namespace MyOverrideExNamespace
{
	[Target("MyTarget")]
	public class MyTarget : Target
	{
		protected override void Write(LogEventInfo logEvent)
		{
			// do nothing
		}
	}
}