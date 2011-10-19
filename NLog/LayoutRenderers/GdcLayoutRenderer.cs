
namespace NLog.LayoutRenderers
{
	using System.Text;
	using NLog.Config;

	/// <summary>
	/// Global Diagnostics Context item. Provided for compatibility with log4net.
	/// </summary>
	[LayoutRenderer("gdc")]
	public class GdcLayoutRenderer : LayoutRenderer
	{
		/// <summary>
		/// Gets or sets the name of the item.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[RequiredParameter]
		[DefaultParameter]
		public string Item { get; set; }

		/// <summary>
		/// Renders the specified Global Diagnostics Context item and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			string msg = GlobalDiagnosticsContext.Get(this.Item);
			builder.Append(msg);
		}
	}
}
