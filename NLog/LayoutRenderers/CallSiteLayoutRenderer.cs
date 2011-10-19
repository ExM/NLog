
namespace NLog.LayoutRenderers
{
	using System.ComponentModel;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using NLog.Config;
	using NLog.Internal;

	/// <summary>
	/// The call site (class name, method name and source information).
	/// </summary>
	[LayoutRenderer("callsite")]
	[ThreadAgnostic]
	public class CallSiteLayoutRenderer : LayoutRenderer, IUsesStackTrace
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CallSiteLayoutRenderer" /> class.
		/// </summary>
		public CallSiteLayoutRenderer()
		{
			this.ClassName = true;
			this.MethodName = true;
			this.FileName = false;
			this.IncludeSourcePath = true;
		}

		/// <summary>
		/// Gets or sets a value indicating whether to render the class name.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(true)]
		public bool ClassName { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to render the method name.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(true)]
		public bool MethodName { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to render the source file name and line number.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(false)]
		public bool FileName { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to include source file path.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(true)]
		public bool IncludeSourcePath { get; set; }

		/// <summary>
		/// Gets the level of stack trace information required by the implementing class.
		/// </summary>
		StackTraceUsage IUsesStackTrace.StackTraceUsage
		{
			get
			{
				if (this.FileName)
				{
					return StackTraceUsage.Max;
				}

				return StackTraceUsage.WithoutSource;
			}
		}

		/// <summary>
		/// Renders the call site and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			StackFrame frame = logEvent.UserStackFrame;
			if (frame != null)
			{
				MethodBase method = frame.GetMethod();
				if (this.ClassName)
				{
					if (method.DeclaringType != null)
					{
						builder.Append(method.DeclaringType.FullName);
					}
					else
					{
						builder.Append("<no type>");
					}
				}

				if (this.MethodName)
				{
					if (this.ClassName)
					{
						builder.Append(".");
					}

					if (method != null)
					{
						builder.Append(method.Name);
					}
					else
					{
						builder.Append("<no method>");
					}
				}

				if (this.FileName)
				{
					string fileName = frame.GetFileName();
					if (fileName != null)
					{
						builder.Append("(");
						if (this.IncludeSourcePath)
						{
							builder.Append(fileName);
						}
						else
						{
							builder.Append(Path.GetFileName(fileName));
						}

						builder.Append(":");
						builder.Append(frame.GetFileLineNumber());
						builder.Append(")");
					}
				}
			}
		}
	}
}

