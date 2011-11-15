
namespace NLog.LayoutRenderers
{
	using System;
	using System.Text;
	using NLog.Common;
	using NLog.Config;
	using NLog.Internal;

	/// <summary>
	/// Render environmental information related to logging events.
	/// </summary>
	[NLogConfigurationItem]
	public abstract class LayoutRenderer : IRenderable
	{
		private const int MaxInitialRenderBufferLength = 16384;
		private int maxRenderedLength;

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			var lra = (LayoutRendererAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(LayoutRendererAttribute));
			if (lra != null)
			{
				return "Layout Renderer: ${" + lra.Name + "}";
			}

			return this.GetType().Name;
		}

		/// <summary>
		/// Initialize for tests
		/// </summary>
		/// <param name="cfg"></param>
		public void Initialize(LoggingConfiguration cfg)
		{
			ObjectGraph.DeepInitialize(this, cfg, true);
		}

		/// <summary>
		/// Renders the the value of layout renderer in the context of the specified log event.
		/// </summary>
		/// <param name="logEvent">The log event.</param>
		/// <returns>String representation of a layout renderer.</returns>
		public string Render(LogEventInfo logEvent)
		{
			int initialLength = this.maxRenderedLength;
			if (initialLength > MaxInitialRenderBufferLength)
			{
				initialLength = MaxInitialRenderBufferLength;
			}

			var builder = new StringBuilder(initialLength);

			this.Render(builder, logEvent);
			if (builder.Length > this.maxRenderedLength)
			{
				this.maxRenderedLength = builder.Length;
			}

			return builder.ToString();
		}
		
		internal void Render(StringBuilder builder, LogEventInfo logEvent)
		{
			try
			{
				this.Append(builder, logEvent);
			}
			catch (Exception exception)
			{
				if (exception.MustBeRethrown())
				{
					throw;
				}

				InternalLogger.Warn("Exception in layout renderer: {0}", exception);
			}
		}

		/// <summary>
		/// Renders the specified environmental information and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected abstract void Append(StringBuilder builder, LogEventInfo logEvent);
	}
}