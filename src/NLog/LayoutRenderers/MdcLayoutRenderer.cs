
namespace NLog.LayoutRenderers
{
	using System.Text;
	using NLog.Config;

	/// <summary>
	/// Mapped Diagnostic Context item. Provided for compatibility with log4net.
	/// </summary>
	[LayoutRenderer("mdc")]
	public class MdcLayoutRenderer : LayoutRenderer
	{
		/// <summary>
		/// Gets or sets the name of the item.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[RequiredParameter]
		[DefaultParameter]
		public string Item { get; set; }

		/// <summary>
		/// Renders the specified MDC item and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			string msg = MappedDiagnosticsContext.Get(this.Item);
			builder.Append(msg);
		}
	}
}
