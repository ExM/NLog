using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using NLog.Common;
using NLog.Config;
using NLog.Internal;
using NLog.Targets;

namespace NLog
{
	/// <summary>
	/// Creates and manages instances of <see cref="T:NLog.Logger" /> objects.
	/// </summary>
	public class LogFactory : IDisposable
	{
		private readonly object _sync = new object();
		private readonly MultiFileWatcher _watcher;
		private const int ReconfigAfterFileChangedTimeout = 1000;

		private readonly Dictionary<LoggerCacheKey, WeakReference> loggerCache = new Dictionary<LoggerCacheKey, WeakReference>();

		private static TimeSpan _defaultFlushTimeout = TimeSpan.FromSeconds(15);

		private Timer _reloadTimer;

		private LoggingConfiguration _config;
		private LogLevel _globalThreshold = LogLevel.MinLevel;
		private bool _configLoaded = false;
		private int _logsEnabled = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="LogFactory" /> class.
		/// </summary>
		public LogFactory()
		{
			_watcher = new MultiFileWatcher();
			_watcher.OnChange += ConfigFileChanged;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LogFactory" /> class.
		/// </summary>
		/// <param name="config">The config.</param>
		public LogFactory(LoggingConfiguration config)
			: this()
		{
			Configuration = config;
		}

		/// <summary>
		/// Occurs when logging <see cref="Configuration" /> changes.
		/// </summary>
		public event EventHandler<LoggingConfigurationChangedEventArgs> ConfigurationChanged;

		/// <summary>
		/// Occurs when logging <see cref="Configuration" /> gets reloaded.
		/// </summary>
		public event EventHandler<LoggingConfigurationReloadedEventArgs> ConfigurationReloaded;

		/// <summary>
		/// Gets or sets a value indicating whether exceptions should be thrown.
		/// </summary>
		/// <value>A value of <c>true</c> if exceptiosn should be thrown; otherwise, <c>false</c>.</value>
		/// <remarks>By default exceptions
		/// are not thrown under any circumstances.
		/// </remarks>
		public bool ThrowExceptions { get; set; }

		/// <summary>
		/// Gets or sets the current logging configuration.
		/// </summary>
		public LoggingConfiguration Configuration
		{
			get
			{
				lock(_sync)
				{
					if(_configLoaded)
						return _config;

					_configLoaded = true;

					if(_config == null) // try to load default configuration
						_config = XmlLoggingConfiguration.AppConfig;

					if(_config == null)
					{
						foreach (string configFile in GetCandidateFileNames())
						{
							if (File.Exists(configFile))
							{
								InternalLogger.Debug("Attempting to load config from {0}", configFile);
								_config = new XmlLoggingConfiguration(configFile);
								break;
							}
						}
					}

					if (_config != null)
					{
						Dump(_config);
						_watcher.Watch(_config.FileNamesToWatch);
						_config.InitializeAll();
					}

					return _config;
				}
			}

			set
			{
				try
				{
					_watcher.StopWatching();
				}
				catch (Exception exception)
				{
					if (exception.MustBeRethrown())
						throw;

					InternalLogger.Error("Cannot stop file watching: {0}", exception);
				}

				lock(_sync)
				{
					LoggingConfiguration oldConfig = _config;
					if (oldConfig != null)
					{
						InternalLogger.Info("Closing old configuration.");
						Flush();
						oldConfig.Close();
					}

					_config = value;
					_configLoaded = true;

					if (_config != null)
					{
						Dump(_config);

						_config.InitializeAll();
						ReconfigExistingLoggers(_config);

						try
						{
							_watcher.Watch(_config.FileNamesToWatch);
						}
						catch (Exception exception)
						{
							if (exception.MustBeRethrown())
								throw;

							InternalLogger.Warn("Cannot start file watching: {0}", exception);
						}
					}

					var configurationChangedDelegate = ConfigurationChanged;

					if (configurationChangedDelegate != null)
					{
						configurationChangedDelegate(this, new LoggingConfigurationChangedEventArgs(oldConfig, value));
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the global log threshold. Log events below this threshold are not logged.
		/// </summary>
		public LogLevel GlobalThreshold
		{
			get
			{
				return _globalThreshold;
			}

			set
			{
				lock(_sync)
				{
					_globalThreshold = value;
					ReconfigExistingLoggers();
				}
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Creates a logger that discards all log messages.
		/// </summary>
		/// <returns>Null logger instance.</returns>
		public Logger CreateNullLogger()
		{
			TargetWithFilterChain[] targetsByLevel = new TargetWithFilterChain[LogLevel.MaxLevel.Ordinal + 1];
			Logger newLogger = new Logger();
			newLogger.Initialize(string.Empty, new LoggerConfiguration(targetsByLevel), this);
			return newLogger;
		}

		/// <summary>
		/// Gets the logger named after the currently-being-initialized class.
		/// </summary>
		/// <returns>The logger.</returns>
		/// <remarks>This is a slow-running method. 
		/// Make sure you're not doing this in a loop.</remarks>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public Logger GetCurrentClassLogger()
		{
			var frame = new StackFrame(1, false);
			return GetLogger(frame.GetMethod().DeclaringType.FullName);
		}

		/// <summary>
		/// Gets the logger named after the currently-being-initialized class.
		/// </summary>
		/// <param name="loggerType">The type of the logger to create. The type must inherit from NLog.Logger.</param>
		/// <returns>The logger.</returns>
		/// <remarks>This is a slow-running method. 
		/// Make sure you're not doing this in a loop.</remarks>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public Logger GetCurrentClassLogger(Type loggerType)
		{

			var frame = new StackFrame(1, false);
			return GetLogger(frame.GetMethod().DeclaringType.FullName, loggerType);
		}

		/// <summary>
		/// Gets the specified named logger.
		/// </summary>
		/// <param name="name">Name of the logger.</param>
		/// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
		public Logger GetLogger(string name)
		{
			return GetLogger(new LoggerCacheKey(typeof(Logger), name));
		}

		/// <summary>
		/// Gets the specified named logger.
		/// </summary>
		/// <param name="name">Name of the logger.</param>
		/// <param name="loggerType">The type of the logger to create. The type must inherit from NLog.Logger.</param>
		/// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the 
		/// same argument aren't guaranteed to return the same logger reference.</returns>
		public Logger GetLogger(string name, Type loggerType)
		{
			return GetLogger(new LoggerCacheKey(loggerType, name));
		}

		/// <summary>
		/// Loops through all loggers previously returned by GetLogger
		/// and recalculates their target and filter list. Useful after modifying the configuration programmatically
		/// to ensure that all loggers have been properly configured.
		/// </summary>
		public void ReconfigExistingLoggers()
		{
			ReconfigExistingLoggers(_config);
		}

		/// <summary>
		/// Flush any pending log messages (in case of asynchronous targets).
		/// </summary>
		public void Flush()
		{
			Flush(_defaultFlushTimeout);
		}

		/// <summary>
		/// Flush any pending log messages (in case of asynchronous targets).
		/// </summary>
		/// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
		public void Flush(TimeSpan timeout)
		{
			AsyncHelpers.RunSynchronously(cb => Flush(cb, timeout));
		}

		/// <summary>
		/// Flush any pending log messages (in case of asynchronous targets).
		/// </summary>
		/// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
		public void Flush(int timeoutMilliseconds)
		{
			Flush(TimeSpan.FromMilliseconds(timeoutMilliseconds));
		}

		/// <summary>
		/// Flush any pending log messages (in case of asynchronous targets).
		/// </summary>
		/// <param name="asyncContinuation">The asynchronous continuation.</param>
		public void Flush(AsyncContinuation asyncContinuation)
		{
			Flush(asyncContinuation, TimeSpan.MaxValue);
		}

		/// <summary>
		/// Flush any pending log messages (in case of asynchronous targets).
		/// </summary>
		/// <param name="asyncContinuation">The asynchronous continuation.</param>
		/// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
		public void Flush(AsyncContinuation asyncContinuation, int timeoutMilliseconds)
		{
			Flush(asyncContinuation, TimeSpan.FromMilliseconds(timeoutMilliseconds));
		}

		/// <summary>
		/// Flush any pending log messages (in case of asynchronous targets).
		/// </summary>
		/// <param name="asyncContinuation">The asynchronous continuation.</param>
		/// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
		public void Flush(AsyncContinuation asyncContinuation, TimeSpan timeout)
		{
			InternalLogger.Trace("LogFactory.Flush({0})", timeout);
			
			var loggingConfiguration = Configuration;
			if (loggingConfiguration != null)
			{
				InternalLogger.Trace("Flushing all targets...");
				loggingConfiguration.FlushAllTargets(AsyncHelpers.WithTimeout(asyncContinuation, timeout));
			}
			else
			{
				asyncContinuation(null);
			}
		}

		/// <summary>Decreases the log enable counter and if it reaches -1 
		/// the logs are disabled.</summary>
		/// <remarks>Logging is enabled if the number of <see cref="EnableLogging"/> calls is greater 
		/// than or equal to <see cref="DisableLogging"/> calls.</remarks>
		/// <returns>An object that iplements IDisposable whose Dispose() method
		/// reenables logging. To be used with C# <c>using ()</c> statement.</returns>
		public IDisposable DisableLogging()
		{
			lock(_sync)
			{
				_logsEnabled--;
				if (_logsEnabled == -1)
				{
					ReconfigExistingLoggers();
				}
			}

			return new LogEnabler(this);
		}

		/// <summary>Increases the log enable counter and if it reaches 0 the logs are disabled.</summary>
		/// <remarks>Logging is enabled if the number of <see cref="EnableLogging"/> calls is greater 
		/// than or equal to <see cref="DisableLogging"/> calls.</remarks>
		public void EnableLogging()
		{
			lock(_sync)
			{
				_logsEnabled++;
				if (_logsEnabled == 0)
				{
					this.ReconfigExistingLoggers();
				}
			}
		}

		/// <summary>
		/// Returns <see langword="true" /> if logging is currently enabled.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is currently enabled, 
		/// <see langword="false"/> otherwise.</returns>
		/// <remarks>Logging is enabled if the number of <see cref="EnableLogging"/> calls is greater 
		/// than or equal to <see cref="DisableLogging"/> calls.</remarks>
		public bool IsLoggingEnabled()
		{
			return _logsEnabled >= 0;
		}

		internal void ReloadConfigOnTimer(object state)
		{
			LoggingConfiguration configurationToReload = (LoggingConfiguration)state;

			InternalLogger.Info("Reloading configuration...");
			lock(_sync)
			{
				if (_reloadTimer != null)
				{
					_reloadTimer.Dispose();
					_reloadTimer = null;
				}

				_watcher.StopWatching();
				try
				{
					if(Configuration != configurationToReload)
						throw new NLogConfigurationException("Config changed in between. Not reloading.");

					LoggingConfiguration newConfig = configurationToReload.Reload();
					if(newConfig == null)
						throw new NLogConfigurationException("Configuration.Reload() returned null. Not reloading.");

					Configuration = newConfig;
					if(ConfigurationReloaded != null)
						ConfigurationReloaded(true, null);
				}
				catch(Exception exception)
				{
					if(exception.MustBeRethrown())
						throw;

					_watcher.Watch(configurationToReload.FileNamesToWatch);

					var configurationReloadedDelegate = ConfigurationReloaded;
					if (configurationReloadedDelegate != null)
						configurationReloadedDelegate(this, new LoggingConfigurationReloadedEventArgs(false, exception));
				}
			}
		}

		internal void ReconfigExistingLoggers(LoggingConfiguration configuration)
		{
			if (configuration != null)
			{
				configuration.EnsureInitialized();
			}

			foreach (var loggerWrapper in this.loggerCache.Values.ToList())
			{
				Logger logger = loggerWrapper.Target as Logger;
				if (logger != null)
				{
					logger.SetConfiguration(this.GetConfigurationForLogger(logger.Name, configuration));
				}
			}
		}

		internal void GetTargetsByLevelForLogger(string name, IList<LoggingRule> rules, TargetWithFilterChain[] targetsByLevel, TargetWithFilterChain[] lastTargetsByLevel)
		{
			foreach (LoggingRule rule in rules)
			{
				if (!rule.NameMatches(name))
					continue;

				for (int i = 0; i <= LogLevel.MaxLevel.Ordinal; ++i)
				{
					if (i < this.GlobalThreshold.Ordinal || !rule.IsLoggingEnabledForLevel(LogLevel.FromOrdinal(i)))
					{
						continue;
					}

					foreach (Target target in rule.Targets)
					{
						var awf = new TargetWithFilterChain(target, rule.Filters);
						if (lastTargetsByLevel[i] != null)
						{
							lastTargetsByLevel[i].NextInChain = awf;
						}
						else
						{
							targetsByLevel[i] = awf;
						}

						lastTargetsByLevel[i] = awf;
					}
				}

				this.GetTargetsByLevelForLogger(name, rule.ChildRules, targetsByLevel, lastTargetsByLevel);

				if (rule.Final)
				{
					break;
				}
			}

			for (int i = 0; i <= LogLevel.MaxLevel.Ordinal; ++i)
			{
				TargetWithFilterChain tfc = targetsByLevel[i];
				if (tfc != null)
				{
					tfc.PrecalculateStackTraceUsage();
				}
			}
		}

		public LoggerConfiguration GetConfigurationForLogger(string name, LoggingConfiguration configuration)
		{
			TargetWithFilterChain[] targetsByLevel = new TargetWithFilterChain[LogLevel.MaxLevel.Ordinal + 1];
			TargetWithFilterChain[] lastTargetsByLevel = new TargetWithFilterChain[LogLevel.MaxLevel.Ordinal + 1];

			if (configuration != null && this.IsLoggingEnabled())
			{
				this.GetTargetsByLevelForLogger(name, configuration.LoggingRules, targetsByLevel, lastTargetsByLevel);
			}

			InternalLogger.Debug("Targets for {0} by level:", name);
			for (int i = 0; i <= LogLevel.MaxLevel.Ordinal; ++i)
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat(CultureInfo.InvariantCulture, "{0} =>", LogLevel.FromOrdinal(i));
				for (TargetWithFilterChain afc = targetsByLevel[i]; afc != null; afc = afc.NextInChain)
				{
					sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", afc.Target.Name);
					if (afc.FilterChain.Count > 0)
					{
						sb.AppendFormat(CultureInfo.InvariantCulture, " ({0} filters)", afc.FilterChain.Count);
					}
				}

				InternalLogger.Debug(sb.ToString());
			}

			return new LoggerConfiguration(targetsByLevel);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing">True to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if(!disposing)
				return;
			
			_watcher.Dispose();

			if(_reloadTimer == null)
				return;
			
			_reloadTimer.Dispose();
			_reloadTimer = null;
		}
		
		internal void Shutdown()
		{
			_watcher.StopWatching();
			
			lock(_sync)
			{
				if (_config == null)
					return;
				
				InternalLogger.Info("Closing configuration.");
				
				
				_config.FlushAllTargets2((ex) =>
				{
					if(ex != null)
						InternalLogger.Error("Flush all targets error: {0}", ex);
				});
				
				_config.Close();
				_config = null;
			}
		}

		private static IEnumerable<string> GetCandidateFileNames()
		{
			// NLog.config from application directory
			yield return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NLog.config");

			// current config file with .config renamed to .nlog
			string cf = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			if (cf != null)
			{
				yield return Path.ChangeExtension(cf, ".nlog");
			}

			// get path to NLog.dll.nlog only if the assembly is not in the GAC
			var nlogAssembly = typeof(LogFactory).Assembly;
			if (!nlogAssembly.GlobalAssemblyCache)
			{
				if (!string.IsNullOrEmpty(nlogAssembly.Location))
				{
					yield return nlogAssembly.Location + ".nlog";
				}
			}
		}

		private static void Dump(LoggingConfiguration config)
		{
			if (!InternalLogger.IsDebugEnabled)
			{
				return;
			}

			config.Dump();
		}

		private Logger GetLogger(LoggerCacheKey cacheKey)
		{
			lock(_sync)
			{
				WeakReference l;

				if (this.loggerCache.TryGetValue(cacheKey, out l))
				{
					Logger existingLogger = l.Target as Logger;
					if (existingLogger != null)
					{
						// logger in the cache and still referenced
						return existingLogger;
					}
				}

				Logger newLogger;

				if (cacheKey.ConcreteType != null && cacheKey.ConcreteType != typeof(Logger))
				{
					newLogger = (Logger)FactoryHelper.CreateInstance(cacheKey.ConcreteType);
				}
				else
				{
					newLogger = new Logger();
				}

				if (cacheKey.ConcreteType != null)
				{
					newLogger.Initialize(cacheKey.Name, this.GetConfigurationForLogger(cacheKey.Name, this.Configuration), this);
				}

				this.loggerCache[cacheKey] = new WeakReference(newLogger);
				return newLogger;
			}
		}

		private void ConfigFileChanged(object sender, EventArgs args)
		{
			InternalLogger.Info("Configuration file change detected! Reloading in {0}ms...", ReconfigAfterFileChangedTimeout);

			// In the rare cases we may get multiple notifications here, 
			// but we need to reload config only once.
			//
			// The trick is to schedule the reload in one second after
			// the last change notification comes in.
			lock(_sync)
			{
				if (_reloadTimer == null)
				{
					_reloadTimer = new Timer(
						ReloadConfigOnTimer,
						Configuration,
						ReconfigAfterFileChangedTimeout,
						Timeout.Infinite);
				}
				else
				{
					_reloadTimer.Change(ReconfigAfterFileChangedTimeout, Timeout.Infinite);
				}
			}
		}

		/// <summary>
		/// Logger cache key.
		/// </summary>
		internal class LoggerCacheKey
		{
			internal LoggerCacheKey(Type loggerConcreteType, string name)
			{
				ConcreteType = loggerConcreteType;
				Name = name;
			}

			internal Type ConcreteType { get; private set; }

			internal string Name { get; private set; }

			/// <summary>
			/// Serves as a hash function for a particular type.
			/// </summary>
			/// <returns>
			/// A hash code for the current <see cref="T:System.Object"/>.
			/// </returns>
			public override int GetHashCode()
			{
				return ConcreteType.GetHashCode() ^ Name.GetHashCode();
			}

			/// <summary>
			/// Determines if two objects are equal in value.
			/// </summary>
			/// <param name="o">Other object to compare to.</param>
			/// <returns>True if objects are equal, false otherwise.</returns>
			public override bool Equals(object o)
			{
				var key = o as LoggerCacheKey;
				if (ReferenceEquals(key, null))
				{
					return false;
				}

				return (ConcreteType == key.ConcreteType) && (key.Name == Name);
			}
		}

		/// <summary>
		/// Enables logging in <see cref="IDisposable.Dispose"/> implementation.
		/// </summary>
		private class LogEnabler : IDisposable
		{
			private LogFactory _factory;

			/// <summary>
			/// Initializes a new instance of the <see cref="LogEnabler" /> class.
			/// </summary>
			/// <param name="factory">The factory.</param>
			public LogEnabler(LogFactory factory)
			{
				_factory = factory;
			}

			/// <summary>
			/// Enables logging.
			/// </summary>
			void IDisposable.Dispose()
			{
				_factory.EnableLogging();
			}
		}
	}
}
