using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace NLog.Internal
{
	internal static class NativeMethods
	{
		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool QueryPerformanceCounter(out ulong lpPerformanceCount);

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool QueryPerformanceFrequency(out ulong lpPerformanceFrequency);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		internal static extern void OutputDebugString(string message);

		[DllImport("kernel32.dll")]
		internal static extern int GetCurrentProcessId();

		[DllImport("kernel32.dll", SetLastError = true, PreserveSig = true, CharSet = CharSet.Unicode)]
		internal static extern uint GetModuleFileName([In] IntPtr hModule, [Out] StringBuilder lpFilename, [In] [MarshalAs(UnmanagedType.U4)] int nSize);
	}
}
