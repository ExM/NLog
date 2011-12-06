using System;
using System.Linq;
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
		private readonly ConfigurationItemFactory _configurationItemFactory;

		private readonly IDictionary<string, Target> _targets =
			new Dictionary<string, Target>(StringComparer.OrdinalIgnoreCase);

		private ISupportsInitialize[] _initializedItems;

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingConfiguration" /> class.
		/// </summary>
		public LoggingConfiguration()
		{
			_configurationItemFactory = new ConfigurationItemFactory(typeof(Logger).Assembly);
			LoggingRules = new List<LoggingRule>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingConfiguration" /> class.
		/// </summary>
		/// <param name="factory">custom factory to resolve references</param>
		public LoggingConfiguration(ConfigurationItemFactory factory)
		{
			_configurationItemFactory = factory;
			LoggingRules = new List<LoggingRule>();
		}
		
		/// <summary>
		/// Instance of <see cref="ConfigurationItemFactory"/> used to resolve references
		/// </summary>
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
			get
			{
				return new List<Target>(_targets.Values).AsReadOnly();
			}
		}

		/// <summary>
		/// Gets the collection of file names which should be watched for changes by NLog.
		/// </summary>
		public virtual IEnumerable<string> FileNamesToWatch
		{
			get
			{
				return new string[0];
			}
		}

		/// <summary>
		/// Gets the collection of logging rules.
		/// </summary>
		public IList<LoggingRule> LoggingRules { get; private set; }

		/// <summary>
		/// Gets all targets.
		/// </summary>
		public Target[] AllTargets
		{
			get
			{
				return ObjectGraph.AllChilds<Target>(_targets.Values).ToArray();
			}
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
				throw new ArgumentException("Target name cannot be null", "name");

			InternalLogger.Debug("Registering target {0}: {1}", name, target.GetType().FullName);
			_targets[name] = target;
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

			if (!_targets.TryGetValue(name, out value))
				return null;

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
			_targets.Remove(name);
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
				throw new ArgumentNullException("installationContext");

			InitializeAll();

			foreach (IInstallable installable in ObjectGraph.AllChilds<IInstallable>(this))
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
						throw;

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
				throw new ArgumentNullException("installationContext");

			InitializeAll();

			foreach (IInstallable installable in ObjectGraph.AllChilds<IInstallable>(this))
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
						throw;

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
			if (_initializedItems != null)
			{
				foreach (var initialize in _initializedItems)
				{
					InternalLogger.Trace("Closing {0}", initialize);
					try
					{
						initialize.Close();
					}
					catch (Exception exception)
					{
						if (exception.MustBeRethrown())
							throw;

						InternalLogger.Warn("Exception while closing {0}", exception);
					}
				}
				_initializedItems = null;
			}

			InternalLogger.Debug("Finished closing logging configuration.");
		}

		internal void Dump()
		{
			InternalLogger.Debug("--- NLog configuration dump. ---");
			InternalLogger.Debug("Targets:");
			foreach (Target target in _targets.Values)
				InternalLogger.Info("{0}", target);

			InternalLogger.Debug("Rules:");
			foreach (LoggingRule rule in LoggingRules)
				InternalLogger.Info("{0}", rule);

			InternalLogger.Debug("--- End of NLog configuration dump ---");
		}

		/// <summary>
		/// Flushes any pending log messages on all appenders.
		/// </summary>
		/// <param name="asyncContinuation">The asynchronous continuation.</param>
		internal void FlushAllTargets(Action<Exception> asyncContinuation)
		{
			var uniqueTargets = new List<Target>();
			foreach(var rule in LoggingRules)
			{
				foreach(var t in rule.Targets)
				{
					if(!uniqueTargets.Contains(t))
						uniqueTargets.Add(t);
				}
			}

			AsyncHelpers.ForEachItemInParallel(uniqueTargets, asyncContinuation, (target, cont) => target.Flush(cont));
		}

		internal void InitializeAll()
		{
			var roots = new List<object>();
			foreach (LoggingRule r in LoggingRules)
				roots.Add(r);

			foreach (Target target in _targets.Values)
				roots.Add(target);

			_initializedItems = roots.DeepInitialize(this, LogManager.ThrowExceptions);

			// initialize all config items starting from most nested first
			// so that whenever the container is initialized its children have already been
			InternalLogger.Info("Found {0} configuration items", _initializedItems.Length);
		}

		/// <summary>
		/// Evaluates the specified text by expadinging all layout renderers
		/// in new <see cref="LogEventInfo" /> context.
		/// </summary>
		/// <param name="text">The text to be evaluated.</param>
		/// <returns>The input text with all occurences of ${} replaced with
		/// values provided by the appropriate layout renderers.</returns>
		public string EvaluateLayout(string text)
		{
			var l = new NLog.Layouts.SimpleLayout(text);
			using (l.Initialize(this))
				return l.Render(LogEventInfo.CreateNullEvent());
		}
	}
}