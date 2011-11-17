using System;
using System.Diagnostics;
using System.Text;
using NLog.Config;
using NLog.Internal;

namespace NLog.LayoutRenderers
{
	/// <summary>
	/// The formatted log message.
	/// </summary>
	[LayoutRenderer("message")]
	[ThreadAgnostic]
	public class MessageLayoutRenderer : LayoutRenderer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MessageLayoutRenderer" /> class.
		/// </summary>
		public MessageLayoutRenderer()
		{
			this.ExceptionSeparator = Environment.NewLine;
		}

		/// <summary>
		/// Gets or sets a value indicating whether to log exception along with message.
		/// </summary>
		/// <docgen category='Layout Options' order='10' />
		public bool WithException { get; set; }

		/// <summary>
		/// Gets or sets the string that separates message from the exception.
		/// </summary>
		/// <docgen category='Layout Options' order='10' />
		public string ExceptionSeparator { get; set; }

		/// <summary>
		/// Renders the log message including any positional parameters and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			builder.Append(logEvent.FormattedMessage);
			if (this.WithException && logEvent.Exception != null)
			{
				builder.Append(this.ExceptionSeparator);
				builder.Append(logEvent.Exception.ToString());
			}
		}
	}
}
