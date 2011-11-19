using System;
using System.ComponentModel;
using NLog.Internal;

namespace NLog
{
	/// <summary>
	/// Provides logging interface and utility functions.
	/// </summary>
	[CLSCompliant(true)]
	public partial class Logger
	{
		private static readonly Type _loggerType = typeof(Logger);

		private volatile LoggerConfiguration _config;

		/// <summary>
		/// Initializes a new instance of the <see cref="Logger"/> class.
		/// </summary>
		protected internal Logger()
		{
		}

		/// <summary>
		/// Occurs when logger configuration changes.
		/// </summary>
		public event EventHandler<EventArgs> LoggerReconfigured;

		/// <summary>
		/// Gets the name of the logger.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the factory that created this logger.
		/// </summary>
		public LogFactory Factory { get; private set; }

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the specified level.
		/// </summary>
		/// <param name="level">Log level to be checked.</param>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the specified level, otherwise it returns <see langword="false" />.</returns>
		public bool IsEnabled(LogLevel level)
		{
			return GetTargetsForLevel(level) != null;
		}

		/// <summary>
		/// Writes the specified diagnostic message.
		/// </summary>
		/// <param name="logEvent">Log event.</param>
		public void Log(LogEventInfo logEvent)
		{
			if(IsEnabled(logEvent.Level))
				WriteToTargets(logEvent);
		}

		/// <summary>
		/// Writes the specified diagnostic message.
		/// </summary>
		/// <param name="wrapperType">The name of the type that wraps Logger.</param>
		/// <param name="logEvent">Log event.</param>
		public void Log(Type wrapperType, LogEventInfo logEvent)
		{
			if(IsEnabled(logEvent.Level))
				WriteToTargets(wrapperType, logEvent);
		}
		
		internal void Initialize(string name, LoggerConfiguration loggerConfiguration, LogFactory factory)
		{
			Name = name;
			Factory = factory;
			SetConfiguration(loggerConfiguration);
		}

		internal void WriteToTargets(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message, object[] args)
		{
			LoggerImpl.Write(_loggerType, GetTargetsForLevel(level),
				LogEventInfo.Create(level, Name, formatProvider, message, args), Factory);
		}

		internal void WriteToTargets<T>(LogLevel level, IFormatProvider formatProvider, T value)
		{
			LoggerImpl.Write(_loggerType, GetTargetsForLevel(level),
				LogEventInfo.Create(level, Name, formatProvider, value), Factory);
		}

		internal void WriteToTargets(LogLevel level, [Localizable(false)] string message, Exception ex)
		{
			LoggerImpl.Write(_loggerType, GetTargetsForLevel(level),
				LogEventInfo.Create(level, Name, message, ex), Factory);
		}

		internal void WriteToTargets(LogEventInfo logEvent)
		{
			LoggerImpl.Write(_loggerType, GetTargetsForLevel(logEvent.Level), logEvent, Factory);
		}

		internal void WriteToTargets(Type wrapperType, LogEventInfo logEvent)
		{
			LoggerImpl.Write(wrapperType, GetTargetsForLevel(logEvent.Level), logEvent, Factory);
		}

		internal void SetConfiguration(LoggerConfiguration newConfiguration)
		{
			_config = newConfiguration;

			// pre-calculate 'enabled' flags
			_isTraceEnabled = newConfiguration.IsEnabled(LogLevel.Trace);
			_isDebugEnabled = newConfiguration.IsEnabled(LogLevel.Debug);
			_isInfoEnabled = newConfiguration.IsEnabled(LogLevel.Info);
			_isWarnEnabled = newConfiguration.IsEnabled(LogLevel.Warn);
			_isErrorEnabled = newConfiguration.IsEnabled(LogLevel.Error);
			_isFatalEnabled = newConfiguration.IsEnabled(LogLevel.Fatal);

			var copy = LoggerReconfigured;

			if (copy != null)
				copy(this, new EventArgs());
		}

		private TargetWithFilterChain GetTargetsForLevel(LogLevel level)
		{
			return _config.GetTargetsForLevel(level);
		}
	}
}
