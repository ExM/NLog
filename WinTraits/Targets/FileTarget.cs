using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using NLog.Common;
using NLog.Config;
using NLog.Internal;
using NLog.Internal.FileAppenders;
using NLog.Layouts;
using System.Runtime.InteropServices;
using NLog.Targets;

namespace NLog.WinTraits.Targets
{
	/// <summary>
	/// Writes log messages to one or more files.
	/// </summary>
	/// <seealso href="http://nlog-project.org/wiki/File_target">Documentation on NLog Wiki</seealso>
	[Target("File")]
	public class FileTarget : NLog.Targets.FileTarget
	{
		/// <summary>
		/// Gets or sets the file attributes (Windows only).
		/// </summary>
		/// <docgen category='Output Options' order='10' />
		[Advanced]
		public Win32FileAttributes FileAttributes { get; set; }

		public FileTarget()
		{
			FileAttributes = Win32FileAttributes.Normal;
		}

		protected override Func<string, BaseFileAppender> ResolveFileAppenderFactory()
		{
			if (ConcurrentWrites && !NetworkWrites && KeepFileOpen)
				return (f) => new MutexMultiProcessFileAppender(f, this);

			return base.ResolveFileAppenderFactory();
		}

		protected override FileStream TryCreateFileStream(string fileName, bool allowConcurrentWrite)
		{
			int fileShare = Win32FileNativeMethods.FILE_SHARE_READ;

			if (allowConcurrentWrite)
				fileShare |= Win32FileNativeMethods.FILE_SHARE_WRITE;

			IntPtr handle = Win32FileNativeMethods.CreateFile(
				fileName,
				Win32FileNativeMethods.FileAccess.GenericWrite,
				fileShare,
				IntPtr.Zero,
				Win32FileNativeMethods.CreationDisposition.OpenAlways,
				FileAttributes,
				IntPtr.Zero);

			if (handle.ToInt32() == -1)
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

			var safeHandle = new Microsoft.Win32.SafeHandles.SafeFileHandle(handle, true);
			var returnValue = new FileStream(safeHandle, FileAccess.Write, BufferSize);
			returnValue.Seek(0, SeekOrigin.End);
			return returnValue;
		}
	}
}
