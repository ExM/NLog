using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace NLog.Internal
{
	/// <summary>
	/// Win32-optimized implementation of <see cref="ThreadIDHelper"/>.
	/// </summary>
	internal static class ThreadIDHelper
	{
		/// <summary>
		/// Gets current process ID.
		/// </summary>
		public static readonly int CurrentProcessID;

		/// <summary>
		/// Gets current process name.
		/// </summary>
		public static readonly string CurrentProcessName;

		/// <summary>
		/// Gets current process name (excluding filename extension, if any).
		/// </summary>
		public static readonly string CurrentProcessBaseName;

		static ThreadIDHelper()
		{
			CurrentProcessID = NativeMethods.GetCurrentProcessId();

			var sb = new StringBuilder(512);
			if (0 == NativeMethods.GetModuleFileName(IntPtr.Zero, sb, sb.Capacity))
				throw new InvalidOperationException("Cannot determine program name.");

			CurrentProcessName = sb.ToString();
			CurrentProcessBaseName = Path.GetFileNameWithoutExtension(CurrentProcessName);
		}
	}
}
