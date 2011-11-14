using NLog.Config;

namespace NLog.Common
{
	/// <summary>
	/// an instance of object is not enough data from the constructor
	/// </summary>
	public interface ISupportsLazyParameters
	{
		/// <summary>
		/// method will be launched before processing child elements
		/// </summary>
		/// <param name="cfg"></param>
		void CreateParameters(LoggingConfiguration cfg);
	}
}
