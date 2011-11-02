using NLog.Internal.FileAppenders;
using NLog.Targets;
using System;

namespace NLog.UnixTraits.Targets
{
	/// <summary>
	/// Writes log messages to one or more files.
	/// </summary>
	/// <seealso href="http://nlog-project.org/wiki/File_target">Documentation on NLog Wiki</seealso>
	[Target("File")]
	public class FileTarget : NLog.Targets.FileTarget
	{
		protected override Func<string, ICreateFileParameters, BaseFileAppender> ResolveFileAppenderFactory()
		{
			if(ConcurrentWrites && !NetworkWrites && KeepFileOpen)
				return (f, p) => new UnixMultiProcessFileAppender(f, p);
			
			return base.ResolveFileAppenderFactory();
		}
	}
}
