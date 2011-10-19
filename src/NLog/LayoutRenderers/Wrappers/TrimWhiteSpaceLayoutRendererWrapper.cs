
namespace NLog.LayoutRenderers.Wrappers
{
	using System.ComponentModel;
	using System.Globalization;
	using NLog.Config;

	/// <summary>
	/// Trims the whitespace from the result of another layout renderer.
	/// </summary>
	[LayoutRenderer("trim-whitespace")]
	[AmbientProperty("TrimWhiteSpace")]
	[ThreadAgnostic]
	public sealed class TrimWhiteSpaceLayoutRendererWrapper : WrapperLayoutRendererBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TrimWhiteSpaceLayoutRendererWrapper" /> class.
		/// </summary>
		public TrimWhiteSpaceLayoutRendererWrapper()
		{
			this.TrimWhiteSpace = true;
		}

		/// <summary>
		/// Gets or sets a value indicating whether lower case conversion should be applied.
		/// </summary>
		/// <value>A value of <c>true</c> if lower case conversion should be applied; otherwise, <c>false</c>.</value>
		/// <docgen category='Transformation Options' order='10' />
		[DefaultValue(true)]
		public bool TrimWhiteSpace { get; set; }

		/// <summary>
		/// Post-processes the rendered message. 
		/// </summary>
		/// <param name="text">The text to be post-processed.</param>
		/// <returns>Trimmed string.</returns>
		protected override string Transform(string text)
		{
			return this.TrimWhiteSpace ? text.Trim() : text;
		}
	}
}
