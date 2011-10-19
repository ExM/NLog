
namespace NLog.LayoutRenderers
{
	using System;
	using System.Diagnostics;
	using System.Text;
	using NLog.Internal;

	/// <summary>
	/// A newline literal.
	/// </summary>
	[LayoutRenderer("newline")]
	public class NewLineLayoutRenderer : LayoutRenderer
	{
		/// <summary>
		/// Renders the specified string literal and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			builder.Append(EnvironmentHelper.NewLine);
		}
	}
}
