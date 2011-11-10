
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
	public abstract class LayoutRenderer : ISupportsInitialize, IRenderable, IDisposable
	{
		private const int MaxInitialRenderBufferLength = 16384;
		private int maxRenderedLength;
		private bool _isInitialized;

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
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
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
		
		/// <summary>
		/// Initializes this instance.
		/// </summary>
		/// <param name="cfg">The configuration.</param>
		public void Initialize(LoggingConfiguration cfg)
		{
			if(_isInitialized)
				return;

			_isInitialized = true;
			InternalInit(cfg);
			
			foreach(var item in ObjectGraph.OneLevelChilds<ISupportsInitialize>(this)) //HACK: cached to close?
				item.Initialize(cfg);
		}

		/// <summary>
		/// Closes this instance.
		/// </summary>
		public void Close()
		{
			if(!_isInitialized)
				return;
			
			_isInitialized = false;
			InternalClose();
			
			foreach(var item in ObjectGraph.OneLevelChilds<ISupportsInitialize>(this))
				item.Close();
		}

		internal void Render(StringBuilder builder, LogEventInfo logEvent)
		{
			if(!_isInitialized)
				throw new InvalidOperationException("required run Initialize method");
				
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

		/// <summary>
		/// Initializes the layout renderer.
		/// </summary>
		protected virtual void InternalInit(LoggingConfiguration cfg)
		{
		}

		/// <summary>
		/// Closes the layout renderer.
		/// </summary>	  
		protected virtual void InternalClose()
		{
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing">True to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.Close();
			}
		}
	}
}