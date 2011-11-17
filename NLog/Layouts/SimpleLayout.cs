using System;
using System.Collections.ObjectModel;
using System.Text;
using NLog.Common;
using NLog.Config;
using NLog.Internal;
using NLog.LayoutRenderers;

namespace NLog.Layouts
{
	/// <summary>
	/// Represents a string with embedded placeholders that can render contextual information.
	/// </summary>
	/// <remarks>
	/// This layout is not meant to be used explicitly. Instead you can just use a string containing layout 
	/// renderers everywhere the layout is required.
	/// </remarks>
	[Layout("SimpleLayout")]
	[ThreadAgnostic]
	[AppDomainFixedOutput]
	public class SimpleLayout : Layout, ISupportsLazyParameters
	{
		private const int MaxInitialRenderBufferLength = 16384;
		private int _maxRenderedLength;

		private string _fixedText;
		private string _layoutText;

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleLayout" /> class.
		/// </summary>
		public SimpleLayout()
			: this(string.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleLayout" /> class.
		/// </summary>
		/// <param name="text">The layout string to parse.</param>
		public SimpleLayout(string text)
		{
			_layoutText = text;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleLayout"/> class.
		/// </summary>
		/// <param name="text">The layout string to parse.</param>
		/// <param name="cfg">The custom configuration for extract renderers</param>
		public SimpleLayout(string text, LoggingConfiguration cfg)
			:this(text, LayoutParser.CompileLayout(cfg, text))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleLayout"/> class.
		/// </summary>
		/// <param name="text">The layout string to view.</param>
		/// <param name="renderers">renderers</param>
		public SimpleLayout(string text, LayoutRenderer[] renderers)
		{
			_layoutText = text;
			SetRenderers(renderers);
		}

		void ISupportsLazyParameters.CreateParameters(LoggingConfiguration cfg)
		{
			if(Renderers == null)
				SetRenderers(LayoutParser.CompileLayout(cfg, _layoutText));
		}

		/// <summary>
		/// Gets or sets the layout text.
		/// </summary>
		/// <docgen category='Layout Options' order='10' />
		public string Text
		{
			get
			{
				return _layoutText;
			}

			set
			{
				_layoutText = value;
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="LayoutRenderer"/> objects that make up this layout.
		/// </summary>
		public ReadOnlyCollection<LayoutRenderer> Renderers { get; private set; }

		/// <summary>
		/// Converts a text to a simple layout.
		/// </summary>
		/// <param name="text">Text to be converted.</param>
		/// <returns>A <see cref="SimpleLayout"/> object.</returns>
		public static implicit operator SimpleLayout(string text)
		{
			return new SimpleLayout(text);
		}

		/// <summary>
		/// Escapes the passed text so that it can
		/// be used literally in all places where
		/// layout is normally expected without being
		/// treated as layout.
		/// </summary>
		/// <param name="text">The text to be escaped.</param>
		/// <returns>The escaped text.</returns>
		/// <remarks>
		/// Escaping is done by replacing all occurences of
		/// '${' with '${literal:text=${}'
		/// </remarks>
		public static string Escape(string text)
		{
			return text.Replace("${", "${literal:text=${}");
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"></see> that represents the current object.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"></see> that represents the current object.
		/// </returns>
		public override string ToString()
		{
			return "'" + Text + "'";
		}

		private void SetRenderers(LayoutRenderer[] renderers)
		{
			Renderers = new ReadOnlyCollection<LayoutRenderer>(renderers);
			if (Renderers.Count == 1 && this.Renderers[0] is LiteralLayoutRenderer)
				_fixedText = ((LiteralLayoutRenderer)Renderers[0]).Text;
			else
				_fixedText = null;
		}

		/// <summary>
		/// Renders the layout for the specified logging event by invoking layout renderers
		/// that make up the event.
		/// </summary>
		/// <param name="logEvent">The logging event.</param>
		/// <returns>The rendered layout.</returns>
		protected override string GetFormattedMessage(LogEventInfo logEvent)
		{
			if(Renderers == null)
				throw new InvalidOperationException("required run CreateParameters method");
		
			if (_fixedText != null)
				return _fixedText;

			string cachedValue;

			if (logEvent.TryGetCachedLayoutValue(this, out cachedValue))
				return cachedValue;

			int initialSize = _maxRenderedLength;
			if (initialSize > MaxInitialRenderBufferLength)
				initialSize = MaxInitialRenderBufferLength;

			var builder = new StringBuilder(initialSize);

			foreach (LayoutRenderer renderer in Renderers)
			{
				try
				{
					renderer.Render(builder, logEvent);
				}
				catch (Exception exception)
				{
					if (exception.MustBeRethrown())
						throw;

					if (InternalLogger.IsWarnEnabled)
						InternalLogger.Warn("Exception in {0}.Append(): {1}.", renderer.GetType().FullName, exception);
				}
			}

			if (builder.Length > _maxRenderedLength)
				_maxRenderedLength = builder.Length;

			string value = builder.ToString();
			logEvent.AddCachedLayoutValue(this, value);
			return value;
		}
	}
}
