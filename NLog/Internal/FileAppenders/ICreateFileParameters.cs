
namespace NLog.Internal.FileAppenders
{
	using NLog.Targets;

	/// <summary>
	/// Interface that provides parameters for create file function.
	/// </summary>
	internal interface ICreateFileParameters
	{
		int ConcurrentWriteAttemptDelay { get; }

		int ConcurrentWriteAttempts { get; }

		bool ConcurrentWrites { get; }

		bool CreateDirs { get; }

		bool EnableFileDelete { get; }

		int BufferSize { get; }

		Win32FileAttributes FileAttributes { get; }
	}
}
