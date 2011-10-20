
namespace NLog.Layouts
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Globalization;
	using System.Text;
	using NLog.Config;

	/// <summary>
	/// A specialized layout that renders CSV-formatted events.
	/// </summary>
	[Layout("CsvLayout")]
	[ThreadAgnostic]
	[AppDomainFixedOutput]
	public class CsvLayout : LayoutWithHeaderAndFooter
	{
		private string actualColumnDelimiter;
		private string doubleQuoteChar;
		private char[] quotableCharacters;

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvLayout"/> class.
		/// </summary>
		public CsvLayout()
		{
			this.Columns = new List<CsvColumn>();
			this.WithHeader = true;
			this.Delimiter = CsvColumnDelimiterMode.Auto;
			this.Quoting = CsvQuotingMode.Auto;
			this.QuoteChar = "\"";
			this.Layout = this;
			this.Header = new CsvHeaderLayout(this);
			this.Footer = null;
		}

		/// <summary>
		/// Gets the array of parameters to be passed.
		/// </summary>
		/// <docgen category='CSV Options' order='10' />
		[ArrayParameter(typeof(CsvColumn), "column")]
		public IList<CsvColumn> Columns { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether CVS should include header.
		/// </summary>
		/// <value>A value of <c>true</c> if CVS should include header; otherwise, <c>false</c>.</value>
		/// <docgen category='CSV Options' order='10' />
		public bool WithHeader { get; set; }

		/// <summary>
		/// Gets or sets the column delimiter.
		/// </summary>
		/// <docgen category='CSV Options' order='10' />
		[DefaultValue("Auto")]
		public CsvColumnDelimiterMode Delimiter { get; set; }

		/// <summary>
		/// Gets or sets the quoting mode.
		/// </summary>
		/// <docgen category='CSV Options' order='10' />
		[DefaultValue("Auto")]
		public CsvQuotingMode Quoting { get; set; }

		/// <summary>
		/// Gets or sets the quote Character.
		/// </summary>
		/// <docgen category='CSV Options' order='10' />
		[DefaultValue("\"")]
		public string QuoteChar { get; set; }

		/// <summary>
		/// Gets or sets the custom column delimiter value (valid when ColumnDelimiter is set to 'Custom').
		/// </summary>
		/// <docgen category='CSV Options' order='10' />
		public string CustomColumnDelimiter { get; set; }

		/// <summary>
		/// Initializes the layout.
		/// </summary>
		protected override void InitializeLayout()
		{
			base.InitializeLayout();
			if (!this.WithHeader)
			{
				this.Header = null;
			}

			switch (this.Delimiter)
			{
				case CsvColumnDelimiterMode.Auto:
					this.actualColumnDelimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
					break;

				case CsvColumnDelimiterMode.Comma:
					this.actualColumnDelimiter = ",";
					break;

				case CsvColumnDelimiterMode.Semicolon:
					this.actualColumnDelimiter = ";";
					break;

				case CsvColumnDelimiterMode.Pipe:
					this.actualColumnDelimiter = "|";
					break;

				case CsvColumnDelimiterMode.Tab:
					this.actualColumnDelimiter = "\t";
					break;

				case CsvColumnDelimiterMode.Space:
					this.actualColumnDelimiter = " ";
					break;

				case CsvColumnDelimiterMode.Custom:
					this.actualColumnDelimiter = this.CustomColumnDelimiter;
					break;
			}

			this.quotableCharacters = (this.QuoteChar + "\r\n" + this.actualColumnDelimiter).ToCharArray();
			this.doubleQuoteChar = this.QuoteChar + this.QuoteChar;
		}

		/// <summary>
		/// Formats the log event for write.
		/// </summary>
		/// <param name="logEvent">The log event to be formatted.</param>
		/// <returns>A string representation of the log event.</returns>
		protected override string GetFormattedMessage(LogEventInfo logEvent)
		{
			string cachedValue;

			if (logEvent.TryGetCachedLayoutValue(this, out cachedValue))
			{
				return cachedValue;
			}

			var sb = new StringBuilder();
			bool first = true;

			foreach (CsvColumn col in this.Columns)
			{
				if (!first)
				{
					sb.Append(this.actualColumnDelimiter);
				}

				first = false;

				bool useQuoting;
				string text = col.Layout.Render(logEvent);

				switch (this.Quoting)
				{
					case CsvQuotingMode.Nothing:
						useQuoting = false;
						break;

					case CsvQuotingMode.All:
						useQuoting = true;
						break;

					default:
					case CsvQuotingMode.Auto:
						if (text.IndexOfAny(this.quotableCharacters) >= 0)
						{
							useQuoting = true;
						}
						else
						{
							useQuoting = false;
						}

						break;
				}

				if (useQuoting)
				{
					sb.Append(this.QuoteChar);
				}

				if (useQuoting)
				{
					sb.Append(text.Replace(this.QuoteChar, this.doubleQuoteChar));
				}
				else
				{
					sb.Append(text);
				}

				if (useQuoting)
				{
					sb.Append(this.QuoteChar);
				}
			}

			return logEvent.AddCachedLayoutValue(this, sb.ToString());
		}

		private string GetHeader()
		{
			var sb = new StringBuilder();

			bool first = true;

			foreach (CsvColumn col in this.Columns)
			{
				if (!first)
				{
					sb.Append(this.actualColumnDelimiter);
				}

				first = false;

				bool useQuoting;
				string text = col.Name;

				switch (this.Quoting)
				{
					case CsvQuotingMode.Nothing:
						useQuoting = false;
						break;

					case CsvQuotingMode.All:
						useQuoting = true;
						break;

					default:
					case CsvQuotingMode.Auto:
						if (text.IndexOfAny(this.quotableCharacters) >= 0)
						{
							useQuoting = true;
						}
						else
						{
							useQuoting = false;
						}

						break;
				}

				if (useQuoting)
				{
					sb.Append(this.QuoteChar);
				}

				if (useQuoting)
				{
					sb.Append(text.Replace(this.QuoteChar, this.doubleQuoteChar));
				}
				else
				{
					sb.Append(text);
				}

				if (useQuoting)
				{
					sb.Append(this.QuoteChar);
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Header for CSV layout.
		/// </summary>
		[ThreadAgnostic]
		private class CsvHeaderLayout : Layout
		{
			private CsvLayout parent;

			/// <summary>
			/// Initializes a new instance of the <see cref="CsvHeaderLayout"/> class.
			/// </summary>
			/// <param name="parent">The parent.</param>
			public CsvHeaderLayout(CsvLayout parent)
			{
				this.parent = parent;
			}

			/// <summary>
			/// Renders the layout for the specified logging event by invoking layout renderers.
			/// </summary>
			/// <param name="logEvent">The logging event.</param>
			/// <returns>The rendered layout.</returns>
			protected override string GetFormattedMessage(LogEventInfo logEvent)
			{
				string cached;

				if (logEvent.TryGetCachedLayoutValue(this, out cached))
				{
					return cached;
				}

				return logEvent.AddCachedLayoutValue(this, this.parent.GetHeader());
			}
		}
	}
}