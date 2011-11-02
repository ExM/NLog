using NLog.Internal.FileAppenders;

namespace NLog.UnixTraits.Targets
{
	/// <summary>
	/// Writes log messages to one or more files.
	/// </summary>
	/// <seealso href="http://nlog-project.org/wiki/File_target">Documentation on NLog Wiki</seealso>
	[Target("File")]
	public class FileTarget : NLog.Targets.FileTarget
	{
		protected override IFileAppenderFactory ResolveFileAppenderFactory()
		{
			if(ConcurrentWrites && !NetworkWrites && KeepFileOpen)
				return UnixMultiProcessFileAppender.TheFactory;
			
			return base.ResolveFileAppenderFactory();
		}
	}
}
