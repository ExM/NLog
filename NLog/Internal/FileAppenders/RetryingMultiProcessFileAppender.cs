
namespace NLog.Internal.FileAppenders
{
	using System;
	using System.IO;
	using NLog.Targets;

	/// <summary>
	/// Multi-process and multi-host file appender which attempts
	/// to get exclusive write access and retries if it's not available.
	/// </summary>
	internal class RetryingMultiProcessFileAppender : BaseFileAppender
	{
		private FileTarget _target;

		/// <summary>
		/// Initializes a new instance of the <see cref="RetryingMultiProcessFileAppender" /> class.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="target">The file target.</param>
		public RetryingMultiProcessFileAppender(string fileName, FileTarget target)
			: base(fileName)
		{
			_target = target;
		}

		/// <summary>
		/// Writes the specified bytes.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		public override void Write(byte[] bytes)
		{
			using (FileStream fileStream = _target.CreateFileStream(FileName, false))
			{
				fileStream.Write(bytes, 0, bytes.Length);
			}

			FileTouched();
		}

		/// <summary>
		/// Flushes this instance.
		/// </summary>
		public override void Flush()
		{
			// nothing to do
		}

		/// <summary>
		/// Closes this instance.
		/// </summary>
		public override void Close()
		{
			// nothing to do
		}

		/// <summary>
		/// Gets the file info.
		/// </summary>
		/// <param name="lastWriteTime">The last write time.</param>
		/// <param name="fileLength">Length of the file.</param>
		/// <returns>
		/// True if the operation succeeded, false otherwise.
		/// </returns>
		public override bool GetFileInfo(out DateTime lastWriteTime, out long fileLength)
		{
			FileInfo fi = new FileInfo(FileName);
			if (fi.Exists)
			{
				fileLength = fi.Length;
				lastWriteTime = fi.LastWriteTime;
				return true;
			}
			else
			{
				fileLength = -1;
				lastWriteTime = DateTime.MinValue;
				return false;
			}
		}
	}
}
