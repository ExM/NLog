using System;
using System.Collections.Generic;
using System.Text;
using NLog.Common;
using NLog.Internal;

namespace NLog.Targets.Wrappers
{
	/// <summary>
	/// A base class for targets which wrap other (multiple) targets
	/// and provide various forms of target routing.
	/// </summary>
	public abstract class CompoundTargetBase : Target
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CompoundTargetBase" /> class.
		/// </summary>
		/// <param name="targets">The targets.</param>
		protected CompoundTargetBase(params Target[] targets)
		{
			this.Targets = new List<Target>(targets);
		}

		/// <summary>
		/// Gets the collection of targets managed by this compound target.
		/// </summary>
		public IList<Target> Targets { get; private set; }

		/// <summary>
		/// Returns the text representation of the object. Used for diagnostics.
		/// </summary>
		/// <returns>A string that describes the target.</returns>
		public override string ToString()
		{
			string separator = string.Empty;
			var sb = new StringBuilder();
			sb.Append(base.ToString());
			sb.Append("(");

			foreach (var t in this.Targets)
			{
				sb.Append(separator);
				sb.Append(t.ToString());
				separator = ", ";
			}

			sb.Append(")");
			return sb.ToString();
		}

		/// <summary>
		/// Writes logging event to the log target.
		/// </summary>
		/// <param name="logEvent">Logging event to be written out.</param>
		protected override void Write(LogEventInfo logEvent)
		{
			throw new NotSupportedException("This target must not be invoked in a synchronous way.");
		}

		/// <summary>
		/// Flush any pending log messages for all wrapped targets.
		/// </summary>
		/// <param name="asyncContinuation">The asynchronous continuation.</param>
		protected override void FlushAsync(AsyncContinuation asyncContinuation)
		{
			AsyncHelpers.ForEachItemInParallel(this.Targets, asyncContinuation, (t, c) => t.Flush(c));
		}
	}
}