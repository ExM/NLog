using NLog.Config;

namespace NLog.Common
{
	public interface ISupportsLazyParameters
	{
		void CreateParameters(LoggingConfiguration cfg);
	}
}
