
namespace NLog.Internal
{
	using NLog.Config;

	/// <summary>
	/// Supports object initialization and termination.
	/// </summary>
	internal interface ISupportsInitialize
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
