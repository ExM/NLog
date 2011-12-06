using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System.Text;
using System.Threading;

class Example
{
	static void Main(string[] args)
	{
		FileTarget target = new FileTarget();
		target.Layout = "${longdate} ${logger} ${message}";
		target.FileName = "${basedir}/logs/logfile.txt";
		target.KeepFileOpen = false;
		target.Encoding = Encoding.UTF8;

		AsyncTargetWrapper wrapper = new AsyncTargetWrapper();
		wrapper.WrappedTarget = target;
		wrapper.QueueLimit = 5000;
		wrapper.OverflowAction = AsyncTargetWrapperOverflowAction.Discard;

		NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(wrapper, LogLevel.Debug);

		Logger logger = LogManager.GetLogger("Example");
		logger.Debug("log message");

		var wait = new ManualResetEvent(false);
		wrapper.Flush((ex) =>{ wait.Set();});
		wait.WaitOne();
	}
}
