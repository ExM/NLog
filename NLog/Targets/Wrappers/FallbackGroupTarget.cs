using System;
using NLog.Common;
using NLog.Internal;

namespace NLog.Targets.Wrappers
{
	/// <summary>
	/// Provides fallback-on-error.
	/// </summary>
	/// <seealso href="http://nlog-project.org/wiki/FallbackGroup_target">Documentation on NLog Wiki</seealso>
	/// <example>
	/// <p>This example causes the messages to be written to server1, 
	/// and if it fails, messages go to server2.</p>
	/// <p>
	/// To set up the target in the <a href="config.html">configuration file</a>, 
	/// use the following syntax:
	/// </p>
	/// <code lang="XML" source="examples/targets/Configuration File/FallbackGroup/NLog.config" />
	/// <p>
	/// The above examples assume just one target and a single rule. See below for
	/// a programmatic configuration that's equivalent to the above config file:
	/// </p>
	/// <code lang="C#" source="examples/targets/Configuration API/FallbackGroup/Simple/Example.cs" />
	/// </example>
	[Target("FallbackGroup", IsCompound = true)]
	public class FallbackGroupTarget : CompoundTargetBase
	{
		private int currentTarget;
		private object lockObject = new object();

		/// <summary>
		/// Initializes a new instance of the <see cref="FallbackGroupTarget"/> class.
		/// </summary>
		public FallbackGroupTarget()
			: this(new Target[0])
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FallbackGroupTarget" /> class.
		/// </summary>
		/// <param name="targets">The targets.</param>
		public FallbackGroupTarget(params Target[] targets)
			: base(targets)
		{
		}

		/// <summary>
		/// Gets or sets a value indicating whether to return to the first target after any successful write.
		/// </summary>
		/// <docgen category='Fallback Options' order='10' />
		public bool ReturnToFirstOnSuccess { get; set; }

		/// <summary>
		/// Forwards the log event to the sub-targets until one of them succeeds.
		/// </summary>
		/// <param name="logEvent">The log event.</param>
		/// <remarks>
		/// The method remembers the last-known-successful target
		/// and starts the iteration from it.
		/// If <see cref="ReturnToFirstOnSuccess"/> is set, the method
		/// resets the target to the first target
		/// stored in <see cref="Targets"/>.
		/// </remarks>
		protected override void Write(AsyncLogEventInfo logEvent)
		{
			Action<Exception> continuation = null;
			int tryCounter = 0;
			int targetToInvoke;

			continuation = ex =>
				{
					if (ex == null)
					{
						// success
						lock (this.lockObject)
						{
							if (this.currentTarget != 0)
							{
								if (this.ReturnToFirstOnSuccess)
								{
									InternalLogger.Debug("Fallback: target '{0}' succeeded. Returning to the first one.", this.Targets[this.currentTarget]);
									this.currentTarget = 0;
								}
							}
						}

						logEvent.Continuation(null);
						return;
					}

					// failure
					lock (this.lockObject)
					{
						InternalLogger.Warn("Fallback: target '{0}' failed. Proceeding to the next one. Error was: {1}", this.Targets[this.currentTarget], ex);

						// error while writing, go to the next one
						this.currentTarget = (this.currentTarget + 1) % this.Targets.Count;

						tryCounter++;
						targetToInvoke = this.currentTarget;
						if (tryCounter >= this.Targets.Count)
						{
							targetToInvoke = -1;
						}
					}

					if (targetToInvoke >= 0)
					{
						this.Targets[targetToInvoke].WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(continuation));
					}
					else
					{
						logEvent.Continuation(ex);
					}
				};

			lock (this.lockObject)
			{
				targetToInvoke = this.currentTarget;
			}

			this.Targets[targetToInvoke].WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(continuation));
		}
	}
}
