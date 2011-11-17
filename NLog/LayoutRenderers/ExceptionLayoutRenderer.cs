using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using NLog.Common;
using NLog.Config;

namespace NLog.LayoutRenderers
{
	/// <summary>
	/// Exception information provided through 
	/// a call to one of the Logger.*Exception() methods.
	/// </summary>
	[LayoutRenderer("exception")]
	[ThreadAgnostic]
	public class ExceptionLayoutRenderer : LayoutRenderer
	{
		private string format;
		private string innerFormat = string.Empty;
		private ExceptionDataTarget[] exceptionDataTargets;
		private ExceptionDataTarget[] innerExceptionDataTargets;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ExceptionLayoutRenderer" /> class.
		/// </summary>
		public ExceptionLayoutRenderer()
		{
			this.Format = "message";
			this.Separator = " ";
			this.InnerExceptionSeparator = Environment.NewLine;
			this.MaxInnerExceptionLevel = 0;
		}

		private delegate void ExceptionDataTarget(StringBuilder sb, Exception ex);

		/// <summary>
		/// Gets or sets the format of the output. Must be a comma-separated list of exception
		/// properties: Message, Type, ShortType, ToString, Method, StackTrace.
		/// This parameter value is case-insensitive.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultParameter]
		public string Format
		{
			get
			{
				return this.format;
			}

			set
			{
				this.format = value;
				this.exceptionDataTargets = CompileFormat(value);
			}
		}

		/// <summary>
		/// Gets or sets the format of the output of inner exceptions. Must be a comma-separated list of exception
		/// properties: Message, Type, ShortType, ToString, Method, StackTrace.
		/// This parameter value is case-insensitive.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		public string InnerFormat
		{
			get
			{
				return this.innerFormat;
			}

			set
			{
				this.innerFormat = value;
				this.innerExceptionDataTargets = CompileFormat(value);
			}
		}

		/// <summary>
		/// Gets or sets the separator used to concatenate parts specified in the Format.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(" ")]
		public string Separator { get; set; }

		/// <summary>
		/// Gets or sets the maximum number of inner exceptions to include in the output.
		/// By default inner exceptions are not enabled for compatibility with NLog 1.0.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(0)]
		public int MaxInnerExceptionLevel { get; set; }

		/// <summary>
		/// Gets or sets the separator between inner exceptions.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		public string InnerExceptionSeparator { get; set; }

		/// <summary>
		/// Renders the specified exception information and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			if (logEvent.Exception != null)
			{
				var sb2 = new StringBuilder(128);
				string separator = string.Empty;

				foreach (ExceptionDataTarget targetRenderFunc in this.exceptionDataTargets)
				{
					sb2.Append(separator);
					targetRenderFunc(sb2, logEvent.Exception);
					separator = this.Separator;
				}

				Exception currentException = logEvent.Exception.InnerException;
				int currentLevel = 0;
				while (currentException != null && currentLevel < this.MaxInnerExceptionLevel)
				{
					// separate inner exceptions
					sb2.Append(this.InnerExceptionSeparator);

					separator = string.Empty;
					foreach (ExceptionDataTarget targetRenderFunc in this.innerExceptionDataTargets ?? this.exceptionDataTargets)
					{
						sb2.Append(separator);
						targetRenderFunc(sb2, currentException);
						separator = this.Separator;
					}

					currentException = currentException.InnerException;
					currentLevel++;
				}

				builder.Append(sb2.ToString());
			}
		}

		private static void AppendMessage(StringBuilder sb, Exception ex)
		{
			sb.Append(ex.Message);
		}

		private static void AppendMethod(StringBuilder sb, Exception ex)
		{
			if (ex.TargetSite != null)
			{
				sb.Append(ex.TargetSite.ToString());
			}
		}

		private static void AppendStackTrace(StringBuilder sb, Exception ex)
		{
			sb.Append(ex.StackTrace);
		}

		private static void AppendToString(StringBuilder sb, Exception ex)
		{
			sb.Append(ex.ToString());
		}

		private static void AppendType(StringBuilder sb, Exception ex)
		{
			sb.Append(ex.GetType().FullName);
		}

		private static void AppendShortType(StringBuilder sb, Exception ex)
		{
			sb.Append(ex.GetType().Name);
		}

		private static ExceptionDataTarget[] CompileFormat(string formatSpecifier)
		{
			string[] parts = formatSpecifier.Replace(" ", string.Empty).Split(',');
			var dataTargets = new List<ExceptionDataTarget>();

			foreach (string s in parts)
			{
				switch (s.ToUpper(CultureInfo.InvariantCulture))
				{
					case "MESSAGE":
						dataTargets.Add(AppendMessage);
						break;

					case "TYPE":
						dataTargets.Add(AppendType);
						break;

					case "SHORTTYPE":
						dataTargets.Add(AppendShortType);
						break;

					case "TOSTRING":
						dataTargets.Add(AppendToString);
						break;

					case "METHOD":
						dataTargets.Add(AppendMethod);
						break;

					case "STACKTRACE":
						dataTargets.Add(AppendStackTrace);
						break;

					default:
						InternalLogger.Warn("Unknown exception data target: {0}", s);
						break;
				}
			}

			return dataTargets.ToArray();
		}
	}
}
