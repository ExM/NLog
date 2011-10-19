
namespace NLog.Filters
{
	using System;
	using System.ComponentModel;
	using NLog.Config;

	/// <summary>
	/// Matches when the calculated layout does NOT contain the specified substring.
	/// This filter is deprecated in favour of <c>&lt;when /&gt;</c> which is based on <a href="conditions.html">contitions</a>.
	/// </summary>
	[Filter("whenNotContains")]
	public class WhenNotContainsFilter : LayoutBasedFilter
	{
		/// <summary>
		/// Gets or sets the substring to be matched.
		/// </summary>
		/// <docgen category='Filtering Options' order='10' />
		[RequiredParameter]
		public string Substring { get; set; }

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
			StringComparison comparison = this.IgnoreCase
											  ? StringComparison.OrdinalIgnoreCase
											  : StringComparison.Ordinal;
			string result = this.Layout.Render(logEvent);

			if (result.IndexOf(this.Substring, comparison) < 0)
			{
				return this.Action;
			}

			return FilterResult.Neutral;
		}
	}
}
