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
	public class SimpleLayout : Layout
	{
		private const int MaxInitialRenderBufferLength = 16384;
		private int _maxRenderedLength;

		private string _fixedText;
		private string _layoutText;
		private ConfigurationItemFactory _configurationItemFactory;

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
		/// <param name="txt">The layout string to parse.</param>
		public SimpleLayout(string txt)
			: this(txt, ConfigurationItemFactory.Default)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleLayout"/> class.
		/// </summary>
		/// <param name="txt">The layout string to parse.</param>
		/// <param name="configurationItemFactory">The NLog factories to use when creating references to layout renderers.</param>
		public SimpleLayout(string txt, ConfigurationItemFactory configurationItemFactory)
		{
			_configurationItemFactory = configurationItemFactory;
			Text = txt;
		}

		public SimpleLayout(LayoutRenderer[] renderers, string text, ConfigurationItemFactory configurationItemFactory)
		{
			_configurationItemFactory = configurationItemFactory;
			SetRenderers(renderers, text);
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
				LayoutRenderer[] renderers;
				string txt;

				renderers = LayoutParser.CompileLayout(
					_configurationItemFactory,
					new SimpleStringReader(value),
					false,
					out txt);

				SetRenderers(renderers, txt);
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
		/// Evaluates the specified text by expadinging all layout renderers.
		/// </summary>
		/// <param name="text">The text to be evaluated.</param>
		/// <param name="logEvent">Log event to be used for evaluation.</param>
		/// <returns>The input text with all occurences of ${} replaced with
		/// values provided by the appropriate layout renderers.</returns>
		public static string Evaluate(string text, LogEventInfo logEvent)
		{
			var l = new SimpleLayout(text);
			return l.Render(logEvent);
		}

		/// <summary>
		/// Evaluates the specified text by expadinging all layout renderers
		/// in new <see cref="LogEventInfo" /> context.
		/// </summary>
		/// <param name="text">The text to be evaluated.</param>
		/// <returns>The input text with all occurences of ${} replaced with
		/// values provided by the appropriate layout renderers.</returns>
		public static string Evaluate(string text)
		{
			return Evaluate(text, LogEventInfo.CreateNullEvent());
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

		private void SetRenderers(LayoutRenderer[] renderers, string text)
		{
			Renderers = new ReadOnlyCollection<LayoutRenderer>(renderers);
			if (Renderers.Count == 1 && this.Renderers[0] is LiteralLayoutRenderer)
			{
				_fixedText = ((LiteralLayoutRenderer)Renderers[0]).Text;
			}
			else
			{
				_fixedText = null;
			}

			_layoutText = text;
		}

		/// <summary>
		/// Renders the layout for the specified logging event by invoking layout renderers
		/// that make up the event.
		/// </summary>
		/// <param name="logEvent">The logging event.</param>
		/// <returns>The rendered layout.</returns>
		protected override string GetFormattedMessage(LogEventInfo logEvent)
		{
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
