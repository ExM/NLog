
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
	/// ASP Session variable.
	/// </summary>
	[LayoutRenderer("asp-session")]
	public class AspSessionValueLayoutRenderer : LayoutRenderer
	{
		/// <summary>
		/// Gets or sets the session variable name.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[RequiredParameter]
		[DefaultParameter]
		public string Variable { get; set; }

		/// <summary>
		/// Renders the specified ASP Session variable and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			var session = AspHelper.GetSessionObject();
			if (session != null)
			{
				if (this.Variable != null)
				{
					object variableValue = session.GetValue(this.Variable);
					builder.Append(Convert.ToString(variableValue, CultureInfo.InvariantCulture));
				}

				Marshal.ReleaseComObject(session);
			}
		}
	}
}

#endif
