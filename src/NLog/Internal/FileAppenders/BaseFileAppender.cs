
namespace NLog.Internal.FileAppenders
{
	using System;
	using System.IO;
	using System.Runtime.InteropServices;
	using NLog.Common;
	using NLog.Config;
	using NLog.Internal;

	/// <summary>
	/// Base class for optimized file appenders.
	/// </summary>
	internal abstract class BaseFileAppender : IDisposable
	{
		private readonly Random random = new Random();

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseFileAppender" /> class.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="createParameters">The create parameters.</param>
		public BaseFileAppender(string fileName, ICreateFileParameters createParameters)
		{
			this.CreateFileParameters = createParameters;
			this.FileName = fileName;
			this.OpenTime = CurrentTimeGetter.Now;
			this.LastWriteTime = DateTime.MinValue;
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
		/// Gets the file creation parameters.
		/// </summary>
		/// <value>The file creation parameters.</value>
		public ICreateFileParameters CreateFileParameters { get; private set; }

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
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing">True to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.Close();
			}
		}

		/// <summary>
		/// Records the last write time for a file.
		/// </summary>
		protected void FileTouched()
		{
			this.LastWriteTime = CurrentTimeGetter.Now;
		}

		/// <summary>
		/// Records the last write time for a file to be specific date.
		/// </summary>
		/// <param name="dateTime">Date and time when the last write occurred.</param>
		protected void FileTouched(DateTime dateTime)
		{
			this.LastWriteTime = dateTime;
		}

		/// <summary>
		/// Creates the file stream.
		/// </summary>
		/// <param name="allowConcurrentWrite">If set to <c>true</c> allow concurrent writes.</param>
		/// <returns>A <see cref="FileStream"/> object which can be used to write to the file.</returns>
		protected FileStream CreateFileStream(bool allowConcurrentWrite)
		{
			int currentDelay = this.CreateFileParameters.ConcurrentWriteAttemptDelay;

			InternalLogger.Trace("Opening {0} with concurrentWrite={1}", this.FileName, allowConcurrentWrite);
			for (int i = 0; i < this.CreateFileParameters.ConcurrentWriteAttempts; ++i)
			{
				try
				{
					try
					{
						return this.TryCreateFileStream(allowConcurrentWrite);
					}
					catch (DirectoryNotFoundException)
					{
						if (!this.CreateFileParameters.CreateDirs)
						{
							throw;
						}

						Directory.CreateDirectory(Path.GetDirectoryName(this.FileName));
						return this.TryCreateFileStream(allowConcurrentWrite);
					}
				}
				catch (IOException)
				{
					if (!this.CreateFileParameters.ConcurrentWrites || !allowConcurrentWrite || i + 1 == this.CreateFileParameters.ConcurrentWriteAttempts)
					{
						throw; // rethrow
					}

					int actualDelay = this.random.Next(currentDelay);
					InternalLogger.Warn("Attempt #{0} to open {1} failed. Sleeping for {2}ms", i, this.FileName, actualDelay);
					currentDelay *= 2;
					System.Threading.Thread.Sleep(actualDelay);
				}
			}

			throw new InvalidOperationException("Should not be reached.");
		}

#if !NET_CF && !SILVERLIGHT
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Objects are disposed elsewhere")]
		private FileStream WindowsCreateFile(string fileName, bool allowConcurrentWrite)
		{
			int fileShare = Win32FileNativeMethods.FILE_SHARE_READ;

			if (allowConcurrentWrite)
			{
				fileShare |= Win32FileNativeMethods.FILE_SHARE_WRITE;
			}

			if (this.CreateFileParameters.EnableFileDelete && PlatformDetector.CurrentOS != RuntimeOS.Windows)
			{
				fileShare |= Win32FileNativeMethods.FILE_SHARE_DELETE;
			}

			IntPtr handle = Win32FileNativeMethods.CreateFile(
				fileName,
				Win32FileNativeMethods.FileAccess.GenericWrite,
				fileShare,
				IntPtr.Zero,
				Win32FileNativeMethods.CreationDisposition.OpenAlways,
				this.CreateFileParameters.FileAttributes, 
				IntPtr.Zero);

			if (handle.ToInt32() == -1)
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}

			var safeHandle = new Microsoft.Win32.SafeHandles.SafeFileHandle(handle, true);
			var returnValue = new FileStream(safeHandle, FileAccess.Write, this.CreateFileParameters.BufferSize);
			returnValue.Seek(0, SeekOrigin.End);
			return returnValue;
		}
#endif

		private FileStream TryCreateFileStream(bool allowConcurrentWrite)
		{
			FileShare fileShare = FileShare.Read;

			if (allowConcurrentWrite)
			{
				fileShare = FileShare.ReadWrite;
			}

#if !NET_CF
			if (this.CreateFileParameters.EnableFileDelete && PlatformDetector.CurrentOS != RuntimeOS.Windows)
			{
				fileShare |= FileShare.Delete;
			}
#endif

#if !NET_CF && !SILVERLIGHT
			if (PlatformDetector.IsDesktopWin32)
			{
				return this.WindowsCreateFile(this.FileName, allowConcurrentWrite);
			}
#endif

			return new FileStream(
				this.FileName, 
				FileMode.Append, 
				FileAccess.Write, 
				fileShare, 
				this.CreateFileParameters.BufferSize);
		}
	}
}
