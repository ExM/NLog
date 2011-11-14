using NLog.Config;

namespace NLog.Internal
{
	public interface ISupportsLazyCast
	{
		void CreateChilds(LoggingConfiguration cfg);
	}
}
