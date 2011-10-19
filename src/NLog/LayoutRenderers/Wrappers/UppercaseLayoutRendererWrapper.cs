
namespace NLog.LayoutRenderers.Wrappers
{
	using System.ComponentModel;
	using System.Globalization;
	using NLog.Config;

	/// <summary>
	/// Converts the result of another layout output to upper case.
	/// </summary>
	[LayoutRenderer("uppercase")]
	[AmbientProperty("Uppercase")]
	[ThreadAgnostic]
	public sealed class UppercaseLayoutRendererWrapper : WrapperLayoutRendererBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UppercaseLayoutRendererWrapper" /> class.
		/// </summary>
		public UppercaseLayoutRendererWrapper()
		{
			this.Culture = CultureInfo.InvariantCulture;
			this.Uppercase = true;
		}

		/// <summary>
		/// Gets or sets a value indicating whether upper case conversion should be applied.
		/// </summary>
		/// <value>A value of <c>true</c> if upper case conversion should be applied otherwise, <c>false</c>.</value>
		/// <docgen category='Transformation Options' order='10' />
		[DefaultValue(true)]
		public bool Uppercase { get; set; }

		/// <summary>
		/// Gets or sets the culture used for rendering. 
		/// </summary>
		/// <docgen category='Transformation Options' order='10' />
		public CultureInfo Culture { get; set; }

		/// <summary>
		/// Post-processes the rendered message. 
		/// </summary>
		/// <param name="text">The text to be post-processed.</param>
		/// <returns>Padded and trimmed string.</returns>
		protected override string Transform(string text)
		{
			return this.Uppercase ? text.ToUpper(this.Culture) : text;
		}
	}
}
