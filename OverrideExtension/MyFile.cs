using NLog;
using NLog.Targets;
	
namespace MyOverrideExNamespace
{
	[Target("File")]
	public class MyFile : Target
	{
		protected override void Write(LogEventInfo logEvent)
		{
			// do nothing
		}
	}
}