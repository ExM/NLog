
namespace NLog.LayoutRenderers
{
	using System;
	using System.Text;

	/// <summary>
	/// Nested Diagnostic Context item. Provided for compatibility with log4net.
	/// </summary>
	[LayoutRenderer("ndc")]
	public class NdcLayoutRenderer : LayoutRenderer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NdcLayoutRenderer" /> class.
		/// </summary>
		public NdcLayoutRenderer()
		{
			this.Separator = " ";
			this.BottomFrames = -1;
			this.TopFrames = -1;
		}

		/// <summary>
		/// Gets or sets the number of top stack frames to be rendered.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		public int TopFrames { get; set; }

		/// <summary>
		/// Gets or sets the number of bottom stack frames to be rendered.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		public int BottomFrames { get; set; }

		/// <summary>
		/// Gets or sets the separator to be used for concatenating nested diagnostics context output.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		public string Separator { get; set; }

		/// <summary>
		/// Renders the specified Nested Diagnostics Context item and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			string[] messages = NestedDiagnosticsContext.GetAllMessages();
			int startPos = 0;
			int endPos = messages.Length;

			if (this.TopFrames != -1)
			{
				endPos = Math.Min(this.TopFrames, messages.Length);
			}
			else if (this.BottomFrames != -1)
			{
				startPos = messages.Length - Math.Min(this.BottomFrames, messages.Length);
			}

			int totalLength = 0;
			int separatorLength = 0;

			for (int i = endPos - 1; i >= startPos; --i)
			{
				totalLength += separatorLength + messages[i].Length;
				separatorLength = this.Separator.Length;
			}

			string separator = string.Empty;

			StringBuilder sb = new StringBuilder();
			for (int i = endPos - 1; i >= startPos; --i)
			{
				sb.Append(separator);
				sb.Append(messages[i]);
				separator = this.Separator;
			}

			builder.Append(sb.ToString());
		}
	}
}
