
namespace NLog.LayoutRenderers
{
	using System.Text;

	/// <summary>
	/// The name of the current thread.
	/// </summary>
	[LayoutRenderer("threadname")]
	public class ThreadNameLayoutRenderer : LayoutRenderer
	{
		/// <summary>
		/// Renders the current thread name and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			builder.Append(System.Threading.Thread.CurrentThread.Name);
		}
	}
}
