using System;
using System.IO;
using System.Runtime.InteropServices;
using NLog.Common;
using NLog.Config;
using NLog.Internal;

namespace NLog.Internal.FileAppenders
{
	/// <summary>
	/// Base class for optimized file appenders.
	/// </summary>
	public abstract class BaseFileAppender : IDisposable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BaseFileAppender" /> class.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="createParameters">The create parameters.</param>
		public BaseFileAppender(string fileName)
		{
			FileName = fileName;
			OpenTime = CurrentTimeGetter.Now;
			LastWriteTime = DateTime.MinValue;
		}

		/// <summary>
		/// Gets the name of the file.
		/// </summary>
		/// <value>The name of the file.</value>
		public string FileName { get; private set; }

		/// <summary>
		/// Gets the last write time.
		/// </summary>
		/// <value>The last write time.</value>
		public DateTime LastWriteTime { get; private set; }

		/// <summary>
		/// Gets the open time of the file.
		/// </summary>
		/// <value>The open time.</value>
		public DateTime OpenTime { get; private set; }

		/// <summary>
		/// Writes the specified bytes.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		public abstract void Write(byte[] bytes);

		/// <summary>
		/// Flushes this instance.
		/// </summary>
		public abstract void Flush();

		/// <summary>
		/// Closes this instance.
		/// </summary>
		public abstract void Close();

		/// <summary>
		/// Gets the file info.
		/// </summary>
		/// <param name="lastWriteTime">The last write time.</param>
		/// <param name="fileLength">Length of the file.</param>
		/// <returns>True if the operation succeeded, false otherwise.</returns>
		public abstract bool GetFileInfo(out DateTime lastWriteTime, out long fileLength);

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing">True to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				Close();
		}

		/// <summary>
		/// Records the last write time for a file.
		/// </summary>
		protected void FileTouched()
		{
			LastWriteTime = CurrentTimeGetter.Now;
		}

		/// <summary>
		/// Records the last write time for a file to be specific date.
		/// </summary>
		/// <param name="dateTime">Date and time when the last write occurred.</param>
		protected void FileTouched(DateTime dateTime)
		{
			LastWriteTime = dateTime;
		}
	}
}
