using System;
using System.Collections.Generic;
using NLog.Common;
using NLog.Config;
using NLog.Internal;

namespace NLog.Targets
{
	/// <summary>
	/// The base class for all targets which call methods (local or remote). 
	/// Manages parameters and type coercion.
	/// </summary>
	public abstract class MethodCallTargetBase : Target
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MethodCallTargetBase" /> class.
		/// </summary>
		protected MethodCallTargetBase()
		{
			this.Parameters = new List<MethodCallParameter>();
		}

		/// <summary>
		/// Gets the array of parameters to be passed.
		/// </summary>
		/// <docgen category='Parameter Options' order='10' />
		[ArrayParameter(typeof(MethodCallParameter), "parameter")]
		public IList<MethodCallParameter> Parameters { get; private set; }

		/// <summary>
		/// Prepares an array of parameters to be passed based on the logging event and calls DoInvoke().
		/// </summary>
		/// <param name="logEvent">
		/// The logging event.
		/// </param>
		protected override void Write(AsyncLogEventInfo logEvent)
		{
			object[] parameters = new object[this.Parameters.Count];
			int i = 0;

			foreach (MethodCallParameter mcp in this.Parameters)
			{
				parameters[i++] = mcp.GetValue(logEvent.LogEvent);
			}

			this.DoInvoke(parameters, logEvent.Continuation);
		}

		/// <summary>
		/// Calls the target method. Must be implemented in concrete classes.
		/// </summary>
		/// <param name="parameters">Method call parameters.</param>
		/// <param name="continuation">The continuation.</param>
		protected virtual void DoInvoke(object[] parameters, Action<Exception> continuation)
		{
			try
			{
				this.DoInvoke(parameters);
				continuation(null);
			}
			catch (Exception ex)
			{
				if (ex.MustBeRethrown())
				{
					throw;
				}

				continuation(ex);
			}
		}

		/// <summary>
		/// Calls the target method. Must be implemented in concrete classes.
		/// </summary>
		/// <param name="parameters">Method call parameters.</param>
		protected abstract void DoInvoke(object[] parameters);
	}
}
