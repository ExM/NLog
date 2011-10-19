
#if !NET_CF && !SILVERLIGHT

namespace NLog.LayoutRenderers
{
	using System;
	using System.Globalization;
	using System.Runtime.InteropServices;
	using System.Text;
	using NLog.Config;
	using NLog.Internal;

	/// <summary>
	/// ASP Application variable.
	/// </summary>
	[LayoutRenderer("asp-application")]
	public class AspApplicationValueLayoutRenderer : LayoutRenderer
	{
		/// <summary>
		/// Gets or sets the ASP Application variable name.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[RequiredParameter]
		[DefaultParameter]
		public string Variable { get; set; }

		/// <summary>
		/// Renders the specified ASP Application variable and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			AspHelper.IApplicationObject app = AspHelper.GetApplicationObject();
			if (app != null)
			{
				if (this.Variable != null)
				{
					object variableValue = app.GetValue(this.Variable);

					builder.Append(Convert.ToString(variableValue, CultureInfo.InvariantCulture));
				}

				Marshal.ReleaseComObject(app);
			}
		}
	}
}
#endif