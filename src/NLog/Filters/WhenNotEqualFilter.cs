
namespace NLog.Filters
{
	using System;
	using System.ComponentModel;
	using NLog.Config;

	/// <summary>
	/// Matches when the calculated layout is NOT equal to the specified substring.
	/// This filter is deprecated in favour of <c>&lt;when /&gt;</c> which is based on <a href="conditions.html">contitions</a>.
	/// </summary>
	[Filter("whenNotEqual")]
	public class WhenNotEqualFilter : LayoutBasedFilter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WhenNotEqualFilter" /> class.
		/// </summary>
		public WhenNotEqualFilter()
		{
		}

		/// <summary>
		/// Gets or sets a string to compare the layout to.
		/// </summary>
		/// <docgen category='Filtering Options' order='10' />
		[RequiredParameter]
		public string CompareTo { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to ignore case when comparing strings.
		/// </summary>
		/// <docgen category='Filtering Options' order='10' />
		[DefaultValue(false)]
		public bool IgnoreCase { get; set; }

		/// <summary>
		/// Checks whether log event should be logged or not.
		/// </summary>
		/// <param name="logEvent">Log event.</param>
		/// <returns>
		/// <see cref="FilterResult.Ignore"/> - if the log event should be ignored<br/>
		/// <see cref="FilterResult.Neutral"/> - if the filter doesn't want to decide<br/>
		/// <see cref="FilterResult.Log"/> - if the log event should be logged<br/>
		/// .</returns>
		protected override FilterResult Check(LogEventInfo logEvent)
		{
			StringComparison comparisonType = this.IgnoreCase
												  ? StringComparison.OrdinalIgnoreCase
												  : StringComparison.Ordinal;

			if (!this.Layout.Render(logEvent).Equals(this.CompareTo, comparisonType))
			{
				return this.Action;
			}

			return FilterResult.Neutral;
		}
	}
}
