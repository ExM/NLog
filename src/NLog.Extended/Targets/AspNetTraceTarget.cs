
#if !NET_CF && !SILVERLIGHT

namespace NLog.Targets
{
	using System.Web;

	/// <summary>
	/// Writes log messages to the ASP.NET trace.
	/// </summary>
	/// <seealso href="http://nlog-project.org/wiki/AspNetTrace_target">Documentation on NLog Wiki</seealso>
	/// <remarks>
	/// Log entries can then be viewed by navigating to http://server/path/Trace.axd.
	/// </remarks>
	[Target("AspNetTrace")]
	public class AspNetTraceTarget : TargetWithLayout
	{
		/// <summary>
		/// Writes the specified logging event to the ASP.NET Trace facility. 
		/// If the log level is greater than or equal to <see cref="LogLevel.Warn"/> it uses the
		/// System.Web.TraceContext.Warn method, otherwise it uses
		/// System.Web.TraceContext.Write method.
		/// </summary>
		/// <param name="logEvent">The logging event.</param>
		protected override void Write(LogEventInfo logEvent)
		{
			HttpContext context = HttpContext.Current;

			if (context == null)
			{
				return;
			}

			if (logEvent.Level >= LogLevel.Warn)
			{
				context.Trace.Warn(logEvent.LoggerName, this.Layout.Render(logEvent));
			}
			else
			{
				context.Trace.Write(logEvent.LoggerName, this.Layout.Render(logEvent));
			}
		}
	}
}

#endif
