using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using NLog.Common;
using NLog.Internal;
using NLog.Targets;
	
namespace NLog.Config
{
	/// <summary>
	/// Keeps logging configuration and provides simple API
	/// to modify it.
	/// </summary>
	public class LoggingConfiguration
	{
		private readonly ConfigurationItemFactory _configurationItemFactory = ConfigurationItemFactory.CreateDefault();

		private readonly IDictionary<string, Target> _targets =
			new Dictionary<string, Target>(StringComparer.OrdinalIgnoreCase);

		private object[] configItems;

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingConfiguration" /> class.
		/// </summary>
		public LoggingConfiguration()
		{
			this.LoggingRules = new List<LoggingRule>();
		}
		
		public ConfigurationItemFactory ItemFactory
		{
			get
			{
				return _configurationItemFactory;
			}
		}

		/// <summary>
		/// Gets a collection of named targets specified in the configuration.
		/// </summary>
		/// <returns>
		/// A list of named targets.
		/// </returns>
		/// <remarks>
		/// Unnamed targets (such as those wrapped by other targets) are not returned.
		/// </remarks>
		public ReadOnlyCollection<Target> ConfiguredNamedTargets
		{
			get { return new List<Target>(this._targets.Values).AsReadOnly(); }
		}

		/// <summary>
		/// Gets the collection of file names which should be watched for changes by NLog.
		/// </summary>
		public virtual IEnumerable<string> FileNamesToWatch
		{
			get { return new string[0]; }
		}

		/// <summary>
		/// Gets the collection of logging rules.
		/// </summary>
		public IList<LoggingRule> LoggingRules { get; private set; }

		/// <summary>
		/// Gets all targets.
		/// </summary>
		public ReadOnlyCollection<Target> AllTargets
		{
			get { return this.configItems.OfType<Target>().ToList().AsReadOnly(); }
		}

		/// <summary>
		/// Registers the specified target object under a given name.
		/// </summary>
		/// <param name="name">
		/// Name of the target.
		/// </param>
		/// <param name="target">
		/// The target object.
		/// </param>
		public void AddTarget(string name, Target target)
		{
			if (name == null)
			{
				throw new ArgumentException("Target name cannot be null", "name");
			}

			InternalLogger.Debug("Registering target {0}: {1}", name, target.GetType().FullName);
			this._targets[name] = target;
		}

		/// <summary>
		/// Finds the target with the specified name.
		/// </summary>
		/// <param name="name">
		/// The name of the target to be found.
		/// </param>
		/// <returns>
		/// Found target or <see langword="null"/> when the target is not found.
		/// </returns>
		public Target FindTargetByName(string name)
		{
			Target value;

			if (!this._targets.TryGetValue(name, out value))
			{
				return null;
			}

			return value;
		}

		/// <summary>
		/// Called by LogManager when one of the log configuration files changes.
		/// </summary>
		/// <returns>
		/// A new instance of <see cref="LoggingConfiguration"/> that represents the updated configuration.
		/// </returns>
		public virtual LoggingConfiguration Reload()
		{
			return this;
		}

		/// <summary>
		/// Removes the specified named target.
		/// </summary>
		/// <param name="name">
		/// Name of the target.
		/// </param>
		public void RemoveTarget(string name)
		{
			this._targets.Remove(name);
		}

		/// <summary>
		/// Installs target-specific objects on current system.
		/// </summary>
		/// <param name="installationContext">The installation context.</param>
		/// <remarks>
		/// Installation typically runs with administrative permissions.
		/// </remarks>
		public void Install(InstallationContext installationContext)
		{
			if (installationContext == null)
			{
				throw new ArgumentNullException("installationContext");
			}

			this.InitializeAll();
			foreach (IInstallable installable in EnumerableHelpers.OfType<IInstallable>(this.configItems))
			{
				installationContext.Info("Installing '{0}'", installable);

				try
				{
					installable.Install(installationContext);
					installationContext.Info("Finished installing '{0}'.", installable);
				}
				catch (Exception exception)
				{
					if (exception.MustBeRethrown())
					{
						throw;
					}

					installationContext.Error("'{0}' installation failed: {1}.", installable, exception);
				}
			}
		}

		/// <summary>
		/// Uninstalls target-specific objects from current system.
		/// </summary>
		/// <param name="installationContext">The installation context.</param>
		/// <remarks>
		/// Uninstallation typically runs with administrative permissions.
		/// </remarks>
		public void Uninstall(InstallationContext installationContext)
		{
			if (installationContext == null)
			{
				throw new ArgumentNullException("installationContext");
			}

			this.InitializeAll();

			foreach (IInstallable installable in EnumerableHelpers.OfType<IInstallable>(this.configItems))
			{
				installationContext.Info("Uninstalling '{0}'", installable);

				try
				{
					installable.Uninstall(installationContext);
					installationContext.Info("Finished uninstalling '{0}'.", installable);
				}
				catch (Exception exception)
				{
					if (exception.MustBeRethrown())
					{
						throw;
					}

					installationContext.Error("Uninstallation of '{0}' failed: {1}.", installable, exception);
				}
			}
		}

		/// <summary>
		/// Closes all targets and releases any unmanaged resources.
		/// </summary>
		internal void Close()
		{
			InternalLogger.Debug("Closing logging configuration...");
			foreach (ISupportsInitialize initialize in this.configItems.OfType<ISupportsInitialize>())
			{
				InternalLogger.Trace("Closing {0}", initialize);
				try
				{
					initialize.Close();
				}
				catch (Exception exception)
				{
					if (exception.MustBeRethrown())
					{
						throw;
					}

					InternalLogger.Warn("Exception while closing {0}", exception);
				}
			}

			InternalLogger.Debug("Finished closing logging configuration.");
		}

		internal void Dump()
		{
			InternalLogger.Debug("--- NLog configuration dump. ---");
			InternalLogger.Debug("Targets:");
			foreach (Target target in this._targets.Values)
			{
				InternalLogger.Info("{0}", target);
			}

			InternalLogger.Debug("Rules:");
			foreach (LoggingRule rule in this.LoggingRules)
			{
				InternalLogger.Info("{0}", rule);
			}

			InternalLogger.Debug("--- End of NLog configuration dump ---");
		}

		/// <summary>
		/// Flushes any pending log messages on all appenders.
		/// </summary>
		/// <param name="asyncContinuation">The asynchronous continuation.</param>
		internal void FlushAllTargets(AsyncContinuation asyncContinuation)
		{
			var uniqueTargets = new List<Target>();
			foreach (var rule in this.LoggingRules)
			{
				foreach (var t in rule.Targets)
				{
					if (!uniqueTargets.Contains(t))
					{
						uniqueTargets.Add(t);
					}
				}
			}

			AsyncHelpers.ForEachItemInParallel(uniqueTargets, asyncContinuation, (target, cont) => target.Flush(cont));
		}
		
		internal void FlushAllTargets2(AsyncContinuation asyncContinuation)
		{
			var uniqueTargets = new List<Target>();
			foreach (var rule in this.LoggingRules)
			foreach (var t in rule.Targets)
				if (!uniqueTargets.Contains(t))
					uniqueTargets.Add(t);
			
			

			//AsyncHelpers.ForEachItemInParallel(uniqueTargets, asyncContinuation, );
			
			AsynchronousAction<Target> action = AsyncHelpers.ExceptionGuard<Target>((target, cont) => target.Flush(cont));

		
			int remaining = uniqueTargets.Count;
			var exceptions = new List<Exception>();

			//InternalLogger.Trace("ForEachItemInParallel() {0} items", items.Count);

			if (remaining == 0)
			{
				asyncContinuation(null);
				return;
			}

			AsyncContinuation continuation =
				ex =>
					{
						InternalLogger.Trace("Continuation invoked: {0}", ex);
						int r;

						if (ex != null)
						{
							lock (exceptions)
							{
								exceptions.Add(ex);
							}
						}

						r = Interlocked.Decrement(ref remaining);
						InternalLogger.Trace("Parallel task completed. {0} items remaining", r);
						if (r == 0)
						{
							asyncContinuation(AsyncHelpers.GetCombinedException(exceptions));
						}
					};
			
			foreach (Target item in uniqueTargets)
				action(item, AsyncHelpers.PreventMultipleCalls(continuation));
			
			//TODO: wait all exit
		}

		/// <summary>
		/// Validates the configuration.
		/// </summary>
		internal void ValidateConfig()
		{
			var roots = new List<object>();
			foreach (LoggingRule r in this.LoggingRules)
			{
				roots.Add(r);
			}

			foreach (Target target in this._targets.Values)
			{
				roots.Add(target);
			}

			this.configItems = ObjectGraphScanner.FindReachableObjects<object>(roots.ToArray());

			// initialize all config items starting from most nested first
			// so that whenever the container is initialized its children have already been
			InternalLogger.Info("Found {0} configuration items", this.configItems.Length);

			foreach (object o in this.configItems)
			{
				PropertyHelper.CheckRequiredParameters(o);
			}
		}

		internal void InitializeAll()
		{
			this.ValidateConfig();

			foreach (ISupportsInitialize initialize in this.configItems.OfType<ISupportsInitialize>().Reverse())
			{
				InternalLogger.Trace("Initializing {0}", initialize);

				try
				{
					initialize.Initialize(this);
				}
				catch (Exception exception)
				{
					if (exception.MustBeRethrown())
					{
						throw;
					}

					if (LogManager.ThrowExceptions)
					{
						throw new NLogConfigurationException("Error during initialization of " + initialize, exception);
					}
				}
			}
		}

		internal void EnsureInitialized()
		{
			this.InitializeAll();
		}
	}
}