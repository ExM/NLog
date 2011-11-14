using NLog.Config;

namespace NLog.Common
{
	/// <summary>
	/// Supports object initialization and termination.
	/// </summary>
	public interface ISupportsInitialize
	{
		/// <summary>
		/// Initializes this instance.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		void Initialize(LoggingConfiguration configuration);

		/// <summary>
		/// Closes this instance.
		/// </summary>
		void Close();
	}
}
