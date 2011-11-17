using System;

namespace NLog.Common
{
	/// <summary>
	/// Detects the platform the NLog is running on.
	/// </summary>
	public static class Platform
	{
		/// <summary>
		/// The current operation system
		/// </summary>
		public static readonly PlatformID CurrentOS;

		static Platform()
		{
			CurrentOS = Environment.OSVersion.Platform;
			if ((int)CurrentOS == 128)
				CurrentOS = PlatformID.Unix;
		}

		/// <summary>
		/// Gets a value indicating whether current OS is a desktop version of Windows.
		/// </summary>
		public static bool IsDesktopWin32
		{
			get
			{
				return
					CurrentOS == PlatformID.Win32Windows ||
					CurrentOS == PlatformID.Win32NT;
			}
		}

		/// <summary>
		/// Gets a value indicating whether current OS is Win32-based (desktop or mobile).
		/// </summary>
		public static bool IsWin32
		{
			get
			{
				return
					CurrentOS == PlatformID.Win32Windows ||
					CurrentOS == PlatformID.Win32NT ||
					CurrentOS == PlatformID.WinCE;
			}
		}
	}
}
