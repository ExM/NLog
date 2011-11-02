

namespace NLog.Internal.FileAppenders
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Threading;
	using NLog.Common;

	/// <summary>
	/// Provides a multiprocess-safe atomic file appends while
	/// keeping the files open.
	/// </summary>
	/// <remarks>
	/// On Unix you can get all the appends to be atomic, even when multiple 
	/// processes are trying to write to the same file, because setting the file
	/// pointer to the end of the file and appending can be made one operation.
	/// On Win32 we need to maintain some synchronization between processes
	/// (global named mutex is used for this)
	/// </remarks>
	internal class MutexMultiProcessFileAppender : BaseFileAppender
	{
		private FileStream file;
		private Mutex mutex;

		/// <summary>
		/// Initializes a new instance of the <see cref="MutexMultiProcessFileAppender" /> class.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="parameters">The parameters.</param>
		public MutexMultiProcessFileAppender(string fileName, ICreateFileParameters parameters) : base(fileName, parameters)
		{
			try
			{
				this.mutex = new Mutex(false, GetMutexName(fileName));
				this.file = CreateFileStream(true);
			}
			catch
			{
				if (this.mutex != null)
				{
					this.mutex.Close();
					this.mutex = null;
				}

				if (this.file != null)
				{
					this.file.Close();
					this.file = null;
				}

				throw;
			}
		}

		/// <summary>
		/// Writes the specified bytes.
		/// </summary>
		/// <param name="bytes">The bytes to be written.</param>
		public override void Write(byte[] bytes)
		{
			if (this.mutex == null)
			{
				return;
			}

			try
			{
				this.mutex.WaitOne();
			}
			catch (AbandonedMutexException)
			{
				// ignore the exception, another process was killed without properly releasing the mutex
				// the mutex has been acquired, so proceed to writing
				// See: http://msdn.microsoft.com/en-us/library/system.threading.abandonedmutexexception.aspx
			}

			try
			{
				this.file.Seek(0, SeekOrigin.End);
				this.file.Write(bytes, 0, bytes.Length);
				this.file.Flush();
				FileTouched();
			}
			finally
			{
				this.mutex.ReleaseMutex();
			}
		}

		/// <summary>
		/// Closes this instance.
		/// </summary>
		public override void Close()
		{
			InternalLogger.Trace("Closing '{0}'", FileName);
			if (this.mutex != null)
			{
				this.mutex.Close();
			}

			if (this.file != null)
			{
				this.file.Close();
			}

			this.mutex = null;
			this.file = null;
			FileTouched();
		}

		/// <summary>
		/// Flushes this instance.
		/// </summary>
		public override void Flush()
		{
			// do nothing, the stream is always flushed
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
			return GetFileInfo_Win(FileName, this.file.SafeFileHandle.DangerousGetHandle(), out lastWriteTime, out fileLength);
		}

		/// <summary>
		/// Gets the information about a file.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="fileHandle">The file handle.</param>
		/// <param name="lastWriteTime">The last write time of the file.</param>
		/// <param name="fileLength">Length of the file.</param>
		/// <returns>
		/// A value of <c>true</c> if file information was retrieved successfully, <c>false</c> otherwise.
		/// </returns>
		private bool GetFileInfo_Win(string fileName, IntPtr fileHandle, out DateTime lastWriteTime, out long fileLength)
		{
			Win32FileNativeMethods.BY_HANDLE_FILE_INFORMATION fi;

			if (Win32FileNativeMethods.GetFileInformationByHandle(fileHandle, out fi))
			{
				lastWriteTime = DateTime.FromFileTime(fi.ftLastWriteTime);
				fileLength = fi.nFileSizeLow + (((long)fi.nFileSizeHigh) << 32);
				return true;
			}
			else
			{
				lastWriteTime = DateTime.MinValue;
				fileLength = -1;
				return false;
			}
		}

		private static string GetMutexName(string fileName)
		{
			string canonicalName = Path.GetFullPath(fileName).ToUpper(CultureInfo.InvariantCulture);

			canonicalName = canonicalName.Replace('\\', '_');
			canonicalName = canonicalName.Replace('/', '_');
			canonicalName = canonicalName.Replace(':', '_');

			return "filelock-mutex-" + canonicalName;
		}
	}
}
