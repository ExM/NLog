using System;
using System.Collections.Generic;
using System.Threading;
using NLog.Common;
using NLog.Internal;

namespace NLog.Targets.Wrappers
{
	/// <summary>
	/// Writes log events to all targets.
	/// </summary>
	/// <seealso href="http://nlog-project.org/wiki/SplitGroup_target">Documentation on NLog Wiki</seealso>
	/// <example>
	/// <p>This example causes the messages to be written to both file1.txt or file2.txt 
	/// </p>
	/// <p>
	/// To set up the target in the <a href="config.html">configuration file</a>, 
	/// use the following syntax:
	/// </p>
	/// <code lang="XML" source="examples/targets/Configuration File/SplitGroup/NLog.config" />
	/// <p>
	/// The above examples assume just one target and a single rule. See below for
	/// a programmatic configuration that's equivalent to the above config file:
	/// </p>
	/// <code lang="C#" source="examples/targets/Configuration API/SplitGroup/Simple/Example.cs" />
	/// </example>
	[Target("SplitGroup", IsCompound = true)]
	public class SplitGroupTarget : CompoundTargetBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SplitGroupTarget" /> class.
		/// </summary>
		public SplitGroupTarget()
			: this(new Target[0])
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SplitGroupTarget" /> class.
		/// </summary>
		/// <param name="targets">The targets.</param>
		public SplitGroupTarget(params Target[] targets)
			: base(targets)
		{
		}

		/// <summary>
		/// Forwards the specified log event to all sub-targets.
		/// </summary>
		/// <param name="logEvent">The log event.</param>
		protected override void Write(AsyncLogEventInfo logEvent)
		{
			AsyncHelpers.ForEachItemSequentially(this.Targets, logEvent.Continuation, (t, cont) => t.WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(cont)));
		}

		/// <summary>
		/// Writes an array of logging events to the log target. By default it iterates on all
		/// events and passes them to "Write" method. Inheriting classes can use this method to
		/// optimize batch writes.
		/// </summary>
		/// <param name="logEvents">Logging events to be written out.</param>
		protected override void Write(AsyncLogEventInfo[] logEvents)
		{
			InternalLogger.Trace("Writing {0} events", logEvents.Length);

			for (int i = 0; i < logEvents.Length; ++i)
			{
				logEvents[i].Continuation = CountedWrap(logEvents[i].Continuation, this.Targets.Count);
			}

			foreach (var t in this.Targets)
			{
				InternalLogger.Trace("Sending {0} events to {1}", logEvents.Length, t);
				t.WriteAsyncLogEvents(logEvents);
			}
		}

		private static Action<Exception> CountedWrap(Action<Exception> originalContinuation, int counter)
		{
			if (counter == 1)
			{
				return originalContinuation;
			}

			var exceptions = new List<Exception>();

			Action<Exception> wrapper =
				ex =>
					{
						if (ex != null)
						{
							lock (exceptions)
							{
								exceptions.Add(ex);
							}
						}

						int c = Interlocked.Decrement(ref counter);

						if (c == 0)
						{
							var combinedException = AsyncHelpers.GetCombinedException(exceptions);
							InternalLogger.Trace("Combined exception: {0}", combinedException);
							originalContinuation(combinedException);
						}
						else
						{
							InternalLogger.Trace("{0} remaining.", c);
						}
					};

			return wrapper;
		}
	}
}
