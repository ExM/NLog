
namespace NLog.Targets.Wrappers
{
	using System;
	using NLog.Common;
	using NLog.Config;
	using NLog.Internal;

	/// <summary>
	/// Base class for targets wrap other (single) targets.
	/// </summary>
	public abstract class WrapperTargetBase : Target
	{
		/// <summary>
		/// Gets or sets the target that is wrapped by this target.
		/// </summary>
		/// <docgen category='General Options' order='11' />
		[RequiredParameter]
		public Target WrappedTarget { get; set; }

		/// <summary>
		/// Returns the text representation of the object. Used for diagnostics.
		/// </summary>
		/// <returns>A string that describes the target.</returns>
		public override string ToString()
		{
			return base.ToString() + "(" + this.WrappedTarget + ")";
		}

		/// <summary>
		/// Flush any pending log messages (in case of asynchronous targets).
		/// </summary>
		/// <param name="asyncContinuation">The asynchronous continuation.</param>
		protected override void FlushAsync(AsyncContinuation asyncContinuation)
		{
			this.WrappedTarget.Flush(asyncContinuation);
		}

		/// <summary>
		/// Writes logging event to the log target. Must be overridden in inheriting
		/// classes.
		/// </summary>
		/// <param name="logEvent">Logging event to be written out.</param>
		protected override sealed void Write(LogEventInfo logEvent)
		{
			throw new NotSupportedException("This target must not be invoked in a synchronous way.");
		}
	}
}