
namespace NLog.LayoutRenderers.Wrappers
{
	using System.ComponentModel;
	using NLog.Config;

	/// <summary>
	/// Applies caching to another layout output.
	/// </summary>
	/// <remarks>
	/// The value of the inner layout will be rendered only once and reused subsequently.
	/// </remarks>
	[LayoutRenderer("cached")]
	[AmbientProperty("Cached")]
	[ThreadAgnostic]
	public sealed class CachedLayoutRendererWrapper : WrapperLayoutRendererBase
	{
		private string cachedValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedLayoutRendererWrapper"/> class.
		/// </summary>
		public CachedLayoutRendererWrapper()
		{
			this.Cached = true;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="CachedLayoutRendererWrapper"/> is enabled.
		/// </summary>
		/// <docgen category='Caching Options' order='10' />
		[DefaultValue(true)]
		public bool Cached { get; set; }

		/// <summary>
		/// Initializes the layout renderer.
		/// </summary>
		protected override void InitializeLayoutRenderer()
		{
			base.InitializeLayoutRenderer();
			this.cachedValue = null;
		}

		/// <summary>
		/// Closes the layout renderer.
		/// </summary>
		protected override void CloseLayoutRenderer()
		{
			base.CloseLayoutRenderer();
			this.cachedValue = null;
		}

		/// <summary>
		/// Transforms the output of another layout.
		/// </summary>
		/// <param name="text">Output to be transform.</param>
		/// <returns>Transformed text.</returns>
		protected override string Transform(string text)
		{
			return text;
		}

		/// <summary>
		/// Renders the inner layout contents.
		/// </summary>
		/// <param name="logEvent">The log event.</param>
		/// <returns>Contents of inner layout.</returns>
		protected override string RenderInner(LogEventInfo logEvent)
		{
			if (this.Cached)
			{
				if (this.cachedValue == null)
				{
					this.cachedValue = base.RenderInner(logEvent);
				}

				return this.cachedValue;
			}
			else
			{
				return base.RenderInner(logEvent);
			}
		}
	}
}
