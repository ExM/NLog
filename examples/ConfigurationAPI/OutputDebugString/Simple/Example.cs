using System;

using NLog;
using NLog.Targets;

class Example
{
	static void Main(string[] args)
	{
		DebugTarget target = new DebugTarget();
		target.Layout = "${message}";

		NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

		Logger logger = LogManager.GetLogger("Example");
		logger.Debug("log message");
	}
}
