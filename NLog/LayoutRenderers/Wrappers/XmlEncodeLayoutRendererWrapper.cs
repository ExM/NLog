
namespace NLog.LayoutRenderers.Wrappers
{
	using System;
	using System.ComponentModel;
	using System.Globalization;
	using System.Text;
	using System.Xml;
	using NLog.Config;

	/// <summary>
	/// Converts the result of another layout output to be XML-compliant.
	/// </summary>
	[LayoutRenderer("xml-encode")]
	[AmbientProperty("XmlEncode")]
	[ThreadAgnostic]
	public sealed class XmlEncodeLayoutRendererWrapper : WrapperLayoutRendererBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="XmlEncodeLayoutRendererWrapper" /> class.
		/// </summary>
		public XmlEncodeLayoutRendererWrapper()
		{
			this.XmlEncode = true;
		}

		/// <summary>
		/// Gets or sets a value indicating whether to apply XML encoding.
		/// </summary>
		/// <docgen category="Transformation Options" order="10"/>
		[DefaultValue(true)]
		public bool XmlEncode { get; set; }

		/// <summary>
		/// Post-processes the rendered message. 
		/// </summary>
		/// <param name="text">The text to be post-processed.</param>
		/// <returns>Padded and trimmed string.</returns>
		protected override string Transform(string text)
		{
			return this.XmlEncode ? DoXmlEscape(text) : text;
		}

		private static string DoXmlEscape(string text)
		{
			var sb = new StringBuilder(text.Length);

			for (int i = 0; i < text.Length; ++i)
			{
				switch (text[i])
				{
					case '<':
						sb.Append("&lt;");
						break;

					case '>':
						sb.Append("&gt;");
						break;

					case '&':
						sb.Append("&amp;");
						break;

					case '\'':
						sb.Append("&apos;");
						break;

					case '"':
						sb.Append("&quot;");
						break;

					default:
						sb.Append(text[i]);
						break;
				}
			}

			return sb.ToString();
		}
	}
}
