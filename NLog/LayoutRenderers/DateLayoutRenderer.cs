using System.ComponentModel;
using System.Globalization;
using System.Text;
using NLog.Config;

namespace NLog.LayoutRenderers
{
	/// <summary>
	/// Current date and time.
	/// </summary>
	[LayoutRenderer("date")]
	[ThreadAgnostic]
	public class DateLayoutRenderer : LayoutRenderer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DateLayoutRenderer" /> class.
		/// </summary>
		public DateLayoutRenderer()
		{
			this.Format = "G";
			this.Culture = CultureInfo.InvariantCulture;
		}

		/// <summary>
		/// Gets or sets the culture used for rendering. 
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		public CultureInfo Culture { get; set; }

		/// <summary>
		/// Gets or sets the date format. Can be any argument accepted by DateTime.ToString(format).
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultParameter]
		public string Format { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to output UTC time instead of local time.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(false)]
		public bool UniversalTime { get; set; }

		/// <summary>
		/// Renders the current date and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			var ts = logEvent.TimeStamp;
			if (this.UniversalTime)
			{
				ts = ts.ToUniversalTime();
			}

			builder.Append(ts.ToString(this.Format, this.Culture));
		}
	}
}
