using System.Collections.Generic;
using NLog.Config;
using NLog.Filters;
using NLog.Targets;
using NLog.Common;

namespace NLog.Internal
{
	/// <summary>
	/// Represents target with a chain of filters which determine
	/// whether logging should happen.
	/// </summary>
	[NLogConfigurationItem]
	public class TargetWithFilterChain
	{
		private StackTraceUsage stackTraceUsage = StackTraceUsage.None;

		/// <summary>
		/// Initializes a new instance of the <see cref="TargetWithFilterChain" /> class.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="filterChain">The filter chain.</param>
		public TargetWithFilterChain(Target target, IList<Filter> filterChain)
		{
			Target = target;
			FilterChain = filterChain;
			stackTraceUsage = StackTraceUsage.None;
		}

		/// <summary>
		/// Gets the target.
		/// </summary>
		/// <value>The target.</value>
		public Target Target { get; private set; }

		/// <summary>
		/// Gets the filter chain.
		/// </summary>
		/// <value>The filter chain.</value>
		public IList<Filter> FilterChain { get; private set; }

		/// <summary>
		/// Gets or sets the next <see cref="TargetWithFilterChain"/> item in the chain.
		/// </summary>
		/// <value>The next item in the chain.</value>
		public TargetWithFilterChain NextInChain { get; set; }

		/// <summary>
		/// Gets the stack trace usage.
		/// </summary>
		/// <returns>A <see cref="StackTraceUsage" /> value that determines stack trace handling.</returns>
		public StackTraceUsage GetStackTraceUsage()
		{
			return stackTraceUsage;
		}

		internal void PrecalculateStackTraceUsage()
		{
			stackTraceUsage = ObjectGraph.PrecalculateStackTraceUsage(this);
		}
	}
}
