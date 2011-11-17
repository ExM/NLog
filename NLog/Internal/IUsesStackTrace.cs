using NLog.Config;

namespace NLog.Internal
{
	/// <summary>
	/// Allows components to request stack trace information to be provided in the <see cref="LogEventInfo"/>.
	/// </summary>
	public interface IUsesStackTrace
	{
		/// <summary>
		/// Gets the level of stack trace information required by the implementing class.
		/// </summary>
		StackTraceUsage StackTraceUsage { get; }
	}
}
