using System.ComponentModel;
using NLog.Config;
using NLog.Internal;
using System;
using NLog.Common;

namespace NLog.Layouts
{
	/// <summary>
	/// Abstract interface that layouts must implement.
	/// </summary>
	[NLogConfigurationItem]
	public abstract class Layout : ISupportsInitialize, IRenderable
	{
		private bool _isInitialized;
		private bool _threadAgnostic;

		/// <summary>
		/// Gets a value indicating whether this layout is thread-agnostic (can be rendered on any thread).
		/// </summary>
		/// <remarks>
		/// Layout is thread-agnostic if it has been marked with [ThreadAgnostic] attribute and all its children are
		/// like that as well.
		/// Thread-agnostic layouts only use contents of <see cref="LogEventInfo"/> for its output.
		/// </remarks>
		public bool IsThreadAgnostic
		{
			get
			{
				return _threadAgnostic;
			}
		}

		/// <summary>
		/// Converts a given text to a <see cref="Layout" />.
		/// </summary>
		/// <param name="text">Text to be converted.</param>
		/// <returns><see cref="SimpleLayout"/> object represented by the text.</returns>
		public static implicit operator Layout([Localizable(false)] string text)
		{
			return new SimpleLayout(text);
		}

		/// <summary>
		/// Precalculates the layout for the specified log event and stores the result
		/// in per-log event cache.
		/// </summary>
		/// <param name="logEvent">The log event.</param>
		/// <remarks>
		/// Calling this method enables you to store the log event in a buffer
		/// and/or potentially evaluate it in another thread even though the 
		/// layout may contain thread-dependent renderer.
		/// </remarks>
		public virtual void Precalculate(LogEventInfo logEvent)
		{
			if (_threadAgnostic)
				return;

			Render(logEvent);
		}

		/// <summary>
		/// Renders the event info in layout.
		/// </summary>
		/// <param name="logEvent">The event info.</param>
		/// <returns>String representing log event.</returns>
		public string Render(LogEventInfo logEvent)
		{
			if (!_isInitialized)
				throw new InvalidOperationException("required run Initialize method");

			return GetFormattedMessage(logEvent);
		}

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		/// <param name="cfg">The configuration.</param>
		public virtual void Initialize(LoggingConfiguration cfg)
		{
			if(_isInitialized)
				return;
			
			_isInitialized = true;
			
			_threadAgnostic = ObjectGraph.ResolveThreadAgnostic(this);
		}

		/// <summary>
		/// Closes this instance.
		/// </summary>
		public virtual void Close()
		{
			if(!_isInitialized)
				return;
			
			_isInitialized = false;
		}

		/// <summary>
		/// Renders the layout for the specified logging event by invoking layout renderers.
		/// </summary>
		/// <param name="logEvent">The logging event.</param>
		/// <returns>The rendered layout.</returns>
		protected abstract string GetFormattedMessage(LogEventInfo logEvent);
	}
}
