
namespace NLog.Internal
{
	using System;
	using System.Runtime.InteropServices;
	using System.Security;
	using System.Text;

	internal static class NativeMethods
	{
		// obtains user token
		[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool LogonUser(string pszUsername, string pszDomain, string pszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

		// closes open handes returned by LogonUser
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CloseHandle(IntPtr handle);

		// creates duplicate token handle
		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool DuplicateToken(IntPtr existingTokenHandle, int impersonationLevel, out IntPtr duplicateTokenHandle);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		internal static extern void OutputDebugString(string message);

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool QueryPerformanceCounter(out ulong lpPerformanceCount);

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool QueryPerformanceFrequency(out ulong lpPerformanceFrequency);

		[DllImport("kernel32.dll")]
		internal static extern int GetCurrentProcessId();

		[DllImport("kernel32.dll", SetLastError = true, PreserveSig = true, CharSet = CharSet.Unicode)]
		internal static extern uint GetModuleFileName([In] IntPtr hModule, [Out] StringBuilder lpFilename, [In] [MarshalAs(UnmanagedType.U4)] int nSize);
	}
}
