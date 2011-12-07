using System;

using NLog;
using NLog.Targets;
using System.Diagnostics;
using NLog.Targets.Wrappers;

class Example
{
    static void Main(string[] args)
    {
        FileTarget file1 = new FileTarget();
        file1.FileName = "${basedir}/file1.txt";

        FileTarget file2 = new FileTarget();
        file2.FileName = "${basedir}/file2.txt";

		RoundRobinGroupTarget target = new RoundRobinGroupTarget();
        target.Targets.Add(file1);
        target.Targets.Add(file2);

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}
