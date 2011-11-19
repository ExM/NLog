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
		private readonly Type loggerType = typeof(Logger);

		private volatile LoggerConfiguration configuration;
		private volatile bool isTraceEnabled;
		private volatile bool isDebugEnabled;
		private volatile bool isInfoEnabled;
		private volatile bool isWarnEnabled;
		private volatile bool isErrorEnabled;
		private volatile bool isFatalEnabled;

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
		/// Gets a value indicating whether logging is enabled for the <c>Trace</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Trace</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsTraceEnabled
		{
			get { return this.isTraceEnabled; }
		}

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Debug</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Debug</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsDebugEnabled
		{
			get { return this.isDebugEnabled; }
		}

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Info</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Info</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsInfoEnabled
		{
			get { return this.isInfoEnabled; }
		}

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Warn</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Warn</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsWarnEnabled
		{
			get { return this.isWarnEnabled; }
		}

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Error</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Error</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsErrorEnabled
		{
			get { return this.isErrorEnabled; }
		}

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Fatal</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Fatal</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsFatalEnabled
		{
			get { return this.isFatalEnabled; }
		}

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the specified level.
		/// </summary>
		/// <param name="level">Log level to be checked.</param>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the specified level, otherwise it returns <see langword="false" />.</returns>
		public bool IsEnabled(LogLevel level)
		{
			return this.GetTargetsForLevel(level) != null;
		}

		/// <summary>
		/// Writes the specified diagnostic message.
		/// </summary>
		/// <param name="logEvent">Log event.</param>
		public void Log(LogEventInfo logEvent)
		{
			if (this.IsEnabled(logEvent.Level))
			{
				this.WriteToTargets(logEvent);
			}
		}

		/// <summary>
		/// Writes the specified diagnostic message.
		/// </summary>
		/// <param name="wrapperType">The name of the type that wraps Logger.</param>
		/// <param name="logEvent">Log event.</param>
		public void Log(Type wrapperType, LogEventInfo logEvent)
		{
			if (this.IsEnabled(logEvent.Level))
			{
				this.WriteToTargets(wrapperType, logEvent);
			}
		}
		
		internal void Initialize(string name, LoggerConfiguration loggerConfiguration, LogFactory factory)
		{
			this.Name = name;
			this.Factory = factory;
			this.SetConfiguration(loggerConfiguration);
		}

		internal void WriteToTargets(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message, object[] args)
		{
			LoggerImpl.Write(this.loggerType, this.GetTargetsForLevel(level), LogEventInfo.Create(level, this.Name, formatProvider, message, args), this.Factory);
		}

		internal void WriteToTargets<T>(LogLevel level, IFormatProvider formatProvider, T value)
		{
			LoggerImpl.Write(this.loggerType, this.GetTargetsForLevel(level), LogEventInfo.Create(level, this.Name, formatProvider, value), this.Factory);
		}

		internal void WriteToTargets(LogLevel level, [Localizable(false)] string message, Exception ex)
		{
			LoggerImpl.Write(this.loggerType, this.GetTargetsForLevel(level), LogEventInfo.Create(level, this.Name, message, ex), this.Factory);
		}

		internal void WriteToTargets(LogLevel level, [Localizable(false)] string message, object[] args)
		{
			this.WriteToTargets(level, null, message, args);
		}

		internal void WriteToTargets(LogEventInfo logEvent)
		{
			LoggerImpl.Write(this.loggerType, this.GetTargetsForLevel(logEvent.Level), logEvent, this.Factory);
		}

		internal void WriteToTargets(Type wrapperType, LogEventInfo logEvent)
		{
			LoggerImpl.Write(wrapperType, this.GetTargetsForLevel(logEvent.Level), logEvent, this.Factory);
		}

		internal void SetConfiguration(LoggerConfiguration newConfiguration)
		{
			this.configuration = newConfiguration;

			// pre-calculate 'enabled' flags
			this.isTraceEnabled = newConfiguration.IsEnabled(LogLevel.Trace);
			this.isDebugEnabled = newConfiguration.IsEnabled(LogLevel.Debug);
			this.isInfoEnabled = newConfiguration.IsEnabled(LogLevel.Info);
			this.isWarnEnabled = newConfiguration.IsEnabled(LogLevel.Warn);
			this.isErrorEnabled = newConfiguration.IsEnabled(LogLevel.Error);
			this.isFatalEnabled = newConfiguration.IsEnabled(LogLevel.Fatal);

			var loggerReconfiguredDelegate = this.LoggerReconfigured;

			if (loggerReconfiguredDelegate != null)
			{
				loggerReconfiguredDelegate(this, new EventArgs());
			}
		}

		private TargetWithFilterChain GetTargetsForLevel(LogLevel level)
		{
			return this.configuration.GetTargetsForLevel(level);
		}
	}
}
