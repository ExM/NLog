using System;
using System.IO;
using NLog.Common;
using NLog.Targets;

namespace NLog.Internal.FileAppenders
{
	/// <summary>
	/// Optimized single-process file appender which keeps the file open for exclusive write.
	/// </summary>
	internal class SingleProcessFileAppender : BaseFileAppender
	{
		private FileStream file;

		/// <summary>
		/// Initializes a new instance of the <see cref="SingleProcessFileAppender" /> class.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="target">The file target.</param>
		public SingleProcessFileAppender(string fileName, FileTarget target)
			: base(fileName)
		{
			this.file = target.CreateFileStream(fileName, false, false);
		}

		/// <summary>
		/// Writes the specified bytes.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		public override void Write(byte[] bytes)
		{
			if (this.file == null)
			{
				return;
			}

			this.file.Write(bytes, 0, bytes.Length);
			FileTouched();
		}

		/// <summary>
		/// Flushes this instance.
		/// </summary>
		public override void Flush()
		{
			if (this.file == null)
			{
				return;
			}

			this.file.Flush();
			FileTouched();
		}

		/// <summary>
		/// Closes this instance.
		/// </summary>
		public override void Close()
		{
			if (this.file == null)
			{
				return;
			}

			InternalLogger.Trace("Closing '{0}'", FileName);
			this.file.Close();
			this.file = null;
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
			throw new NotSupportedException();
		}
	}
}
