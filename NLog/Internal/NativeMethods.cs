
namespace NLog.Internal
{
	using System;
	using System.Runtime.InteropServices;
	using System.Security;
	using System.Text;

	internal static class NativeMethods
	{
		[DllImport("kernel32.dll")]
		internal static extern int GetCurrentProcessId();

		[DllImport("kernel32.dll", SetLastError = true, PreserveSig = true, CharSet = CharSet.Unicode)]
		internal static extern uint GetModuleFileName([In] IntPtr hModule, [Out] StringBuilder lpFilename, [In] [MarshalAs(UnmanagedType.U4)] int nSize);
	}
}
