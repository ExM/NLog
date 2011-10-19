
namespace NLog.Config
{
	using NLog.Targets;

	/// <summary>
	/// Provides simple programmatic configuration API used for trivial logging cases.
	/// </summary>
	public static class SimpleConfigurator
	{
		/// <summary>
		/// Configures NLog for console logging so that all messages above and including
		/// the <see cref="LogLevel.Info"/> level are output to the console.
		/// </summary>
		public static void ConfigureForConsoleLogging()
		{
			ConfigureForConsoleLogging(LogLevel.Info);
		}

		/// <summary>
		/// Configures NLog for console logging so that all messages above and including
		/// the specified level are output to the console.
		/// </summary>
		/// <param name="minLevel">The minimal logging level.</param>
		public static void ConfigureForConsoleLogging(LogLevel minLevel)
		{
			ConsoleTarget consoleTarget = new ConsoleTarget();

			LoggingConfiguration config = new LoggingConfiguration();
			LoggingRule rule = new LoggingRule("*", minLevel, consoleTarget);
			config.LoggingRules.Add(rule);
			LogManager.Configuration = config;
		}

		/// <summary>
		/// Configures NLog for to log to the specified target so that all messages 
		/// above and including the <see cref="LogLevel.Info"/> level are output.
		/// </summary>
		/// <param name="target">The target to log all messages to.</param>
		public static void ConfigureForTargetLogging(Target target)
		{
			ConfigureForTargetLogging(target, LogLevel.Info);
		}

		/// <summary>
		/// Configures NLog for to log to the specified target so that all messages 
		/// above and including the specified level are output.
		/// </summary>
		/// <param name="target">The target to log all messages to.</param>
		/// <param name="minLevel">The minimal logging level.</param>
		public static void ConfigureForTargetLogging(Target target, LogLevel minLevel)
		{
			LoggingConfiguration config = new LoggingConfiguration();
			LoggingRule rule = new LoggingRule("*", minLevel, target);
			config.LoggingRules.Add(rule);
			LogManager.Configuration = config;
		}

		/// <summary>
		/// Configures NLog for file logging so that all messages above and including
		/// the <see cref="LogLevel.Info"/> level are written to the specified file.
		/// </summary>
		/// <param name="fileName">Log file name.</param>
		public static void ConfigureForFileLogging(string fileName)
		{
			ConfigureForFileLogging(fileName, LogLevel.Info);
		}

		/// <summary>
		/// Configures NLog for file logging so that all messages above and including
		/// the specified level are written to the specified file.
		/// </summary>
		/// <param name="fileName">Log file name.</param>
		/// <param name="minLevel">The minimal logging level.</param>
		public static void ConfigureForFileLogging(string fileName, LogLevel minLevel)
		{
			FileTarget target = new FileTarget();
			target.FileName = fileName;
			ConfigureForTargetLogging(target, minLevel);
		}
	}
}
