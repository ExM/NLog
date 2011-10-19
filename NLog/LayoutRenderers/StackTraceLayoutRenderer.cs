
namespace NLog.LayoutRenderers
{
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Text;
	using NLog.Config;

	using NLog.Internal;

	/// <summary>
	/// Stack trace renderer.
	/// </summary>
	[LayoutRenderer("stacktrace")]
	[ThreadAgnostic]
	public class StackTraceLayoutRenderer : LayoutRenderer, IUsesStackTrace
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StackTraceLayoutRenderer" /> class.
		/// </summary>
		public StackTraceLayoutRenderer()
		{
			this.Separator = " => ";
			this.TopFrames = 3;
			this.Format = StackTraceFormat.Flat;
		}

		/// <summary>
		/// Gets or sets the output format of the stack trace.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue("Flat")]
		public StackTraceFormat Format { get; set; }

		/// <summary>
		/// Gets or sets the number of top stack frames to be rendered.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(3)]
		public int TopFrames { get; set; }

		/// <summary>
		/// Gets or sets the stack frame separator string.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(" => ")]
		public string Separator { get; set; }

		/// <summary>
		/// Gets the level of stack trace information required by the implementing class.
		/// </summary>
		/// <value></value>
		StackTraceUsage IUsesStackTrace.StackTraceUsage
		{
			get { return StackTraceUsage.WithoutSource; }
		}

		/// <summary>
		/// Renders the call site and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			bool first = true;
			int startingFrame = logEvent.UserStackFrameNumber + this.TopFrames - 1;
			if (startingFrame >= logEvent.StackTrace.FrameCount)
			{
				startingFrame = logEvent.StackTrace.FrameCount - 1;
			}

			switch (this.Format)
			{
				case StackTraceFormat.Raw:
					for (int i = startingFrame; i >= logEvent.UserStackFrameNumber; --i)
					{
						StackFrame f = logEvent.StackTrace.GetFrame(i);
						builder.Append(f.ToString());
					}

					break;

				case StackTraceFormat.Flat:
					for (int i = startingFrame; i >= logEvent.UserStackFrameNumber; --i)
					{
						StackFrame f = logEvent.StackTrace.GetFrame(i);
						if (!first)
						{
							builder.Append(this.Separator);
						}

						var type = f.GetMethod().DeclaringType;

						if (type != null)
						{
							builder.Append(type.Name);
						}
						else
						{
							builder.Append("<no type>");
						}

						builder.Append(".");
						builder.Append(f.GetMethod().Name);
						first = false;
					}

					break;

				case StackTraceFormat.DetailedFlat:
					for (int i = startingFrame; i >= logEvent.UserStackFrameNumber; --i)
					{
						StackFrame f = logEvent.StackTrace.GetFrame(i);
						if (!first)
						{
							builder.Append(this.Separator);
						}

						builder.Append("[");
						builder.Append(f.GetMethod());
						builder.Append("]");
						first = false;
					}

					break;
			}
		}
	}
}
