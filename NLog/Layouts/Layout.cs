using System.ComponentModel;
using NLog.Config;
using NLog.Internal;
using System;

namespace NLog.Layouts
{
	/// <summary>
	/// Abstract interface that layouts must implement.
	/// </summary>
	[NLogConfigurationItem]
	public abstract class Layout : ISupportsInitialize, IRenderable
	{
		private bool isInitialized;
		private bool threadAgnostic;

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
				return threadAgnostic;
			}
		}

		/// <summary>
		/// Gets the logging configuration this target is part of.
		/// </summary>
		protected LoggingConfiguration LoggingConfiguration { get; private set; }

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
			if (threadAgnostic)
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
			if (!isInitialized)
				throw new InvalidOperationException("required run Initialize method");
			//{
			//	isInitialized = true;
			//	InitializeLayout();
			//}

			return GetFormattedMessage(logEvent);
		}

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		public void Initialize(LoggingConfiguration configuration)
		{
			if (isInitialized)
				return;
			
			LoggingConfiguration = configuration;
			isInitialized = true;
			InitializeLayout();
			
			// determine whether the layout is thread-agnostic
			// layout is thread agnostic if it is thread-agnostic and 
			// all its nested objects are thread-agnostic.
			threadAgnostic = true;
			foreach(object item in ObjectGraphScanner.FindReachableObjects<object>(this))
			{
				if (!item.GetType().IsDefined(typeof(ThreadAgnosticAttribute), true))
				{
					threadAgnostic = false;
					break;
				}
			}
		}

		/// <summary>
		/// Closes this instance.
		/// </summary>
		public void Close()
		{
			if (isInitialized)
			{
				LoggingConfiguration = null;
				isInitialized = false;
				CloseLayout();
			}
		}

		/// <summary>
		/// Initializes the layout.
		/// </summary>
		protected virtual void InitializeLayout()
		{
			//if(LoggingConfiguration == null)
			//	LoggingConfiguration = new LoggingConfiguration(); //TODO: remove after fix all tests (used ConfigurationItemFactory.Default)
		}

		/// <summary>
		/// Closes the layout.
		/// </summary>
		protected virtual void CloseLayout()
		{
		}

		/// <summary>
		/// Renders the layout for the specified logging event by invoking layout renderers.
		/// </summary>
		/// <param name="logEvent">The logging event.</param>
		/// <returns>The rendered layout.</returns>
		protected abstract string GetFormattedMessage(LogEventInfo logEvent);
	}
}
