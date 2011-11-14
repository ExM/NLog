using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog.Config;
using NLog.Common;

namespace NLog.Internal
{
	internal class InitializeContext
	{
		private LoggingConfiguration _cfg;
		private List<ISupportsInitialize> _initialized;
		private HashSet<object> _visited;
		private bool _throwExceptions;

		internal InitializeContext(LoggingConfiguration cfg, bool throwExceptions)
		{
			_cfg = cfg;
			_throwExceptions = throwExceptions;
			_visited = new HashSet<object>();
			_initialized = new List<ISupportsInitialize>();
		}

		internal ISupportsInitialize[] Initialized
		{
			get
			{
				return _initialized.ToArray();
			}
		}

		internal void DeepInitialize(object root)
		{
			if (root == null)
				return;

			if (!_visited.Add(root))
				return;

			if (!root.GetType().IsDefined(typeof(NLogConfigurationItemAttribute), true))
				return;

			ObjectGraph.CheckRequiredParameters(root);
			
			AttemptLazyCast(root as ISupportsLazyParameters);
			
			foreach (object child in ObjectGraph.OneLevelChilds(root))
				DeepInitialize(child);
				
			AttemptInitialize(root as ISupportsInitialize);
		}

		private void AttemptInitialize(ISupportsInitialize supInit)
		{
			if (supInit == null)
				return;

			InternalLogger.Trace("Initializing {0}", supInit);

			try
			{
				supInit.Initialize(_cfg);
				_initialized.Add(supInit);
			}
			catch (Exception exception)
			{
				if (exception.MustBeRethrown())
					throw;

				if (_throwExceptions)
					throw new NLogConfigurationException("Error during initialization of " + supInit, exception);
			}
		}
		
		private void AttemptLazyCast(ISupportsLazyParameters supLazyCast)
		{
			if(supLazyCast != null)
				supLazyCast.CreateParameters(_cfg);
		}
	}
}
