using NLog.Config;
using NLog.Layouts;

namespace NLog.Targets
{
	/// <summary>
	/// Represents a parameter to a NLogViewer target.
	/// </summary>
	[NLogConfigurationItem]
	public class NLogViewerParameterInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NLogViewerParameterInfo" /> class.
		/// </summary>
		public NLogViewerParameterInfo()
		{
		}

		/// <summary>
		/// Gets or sets viewer parameter name.
		/// </summary>
		/// <docgen category='Parameter Options' order='10' />
		[RequiredParameter]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the layout that should be use to calcuate the value for the parameter.
		/// </summary>
		/// <docgen category='Parameter Options' order='10' />
		[RequiredParameter]
		public Layout Layout { get; set; }
	}
}
