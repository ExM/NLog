using NLog.Config;
using System.Diagnostics;
using System.Threading;
using System;
using System.IO;

namespace NLog.Internal
{
	/// <summary>
	/// Returns details about current process and thread in a portable manner.
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
			CurrentProcessID = Process.GetCurrentProcess().Id;
			CurrentProcessName = GetProcessName();
			CurrentProcessBaseName = Path.GetFileNameWithoutExtension(CurrentProcessName);
		}
		
		private static string GetProcessName()
		{
			try
			{
				return Process.GetCurrentProcess().MainModule.FileName;
			}
			catch (Exception exception)
			{
				if (exception.MustBeRethrown())
					throw;

				return "<unknown>";
			}
		}
	}
}
