
namespace NLog.LayoutRenderers
{
	using System.Globalization;
	using System.Text;
	using NLog.Config;
	using NLog.Internal;

	/// <summary>
	/// The identifier of the current process.
	/// </summary>
	[LayoutRenderer("processid")]
	[AppDomainFixedOutput]
	[ThreadAgnostic]
	public class ProcessIdLayoutRenderer : LayoutRenderer
	{
		/// <summary>
		/// Renders the current process ID.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			builder.Append(ThreadIDHelper.Instance.CurrentProcessID.ToString(CultureInfo.InvariantCulture));
		}
	}
}

