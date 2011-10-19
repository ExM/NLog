
namespace NLog.Filters
{
	using NLog.Config;
	using NLog.Layouts;

	/// <summary>
	/// A base class for filters that are based on comparing a value to a layout.
	/// </summary>
	public abstract class LayoutBasedFilter : Filter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutBasedFilter" /> class.
		/// </summary>
		protected LayoutBasedFilter()
		{
		}

		/// <summary>
		/// Gets or sets the layout to be used to filter log messages.
		/// </summary>
		/// <value>The layout.</value>
		/// <docgen category='Filtering Options' order='10' />
		[RequiredParameter]
		public Layout Layout { get; set; }
	}
}
