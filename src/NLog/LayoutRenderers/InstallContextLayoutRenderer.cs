
namespace NLog.LayoutRenderers
{
	using System;
	using System.Globalization;
	using System.Text;
	using NLog.Config;

	/// <summary>
	/// Installation parameter (passed to InstallNLogConfig).
	/// </summary>
	[LayoutRenderer("install-context")]
	public class InstallContextLayoutRenderer : LayoutRenderer
	{
		/// <summary>
		/// Gets or sets the name of the parameter.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[RequiredParameter]
		[DefaultParameter]
		public string Parameter { get; set; }

		/// <summary>
		/// Renders the specified installation parameter and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			object value;

			if (logEvent.Properties.TryGetValue(this.Parameter, out value))
			{
				builder.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
			}
		}
	}
}
