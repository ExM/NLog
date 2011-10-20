
#if !SILVERLIGHT

namespace NLog.Internal
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Threading;

	/// <summary>
	/// Win32-optimized implementation of <see cref="ThreadIDHelper"/>.
	/// </summary>
	internal class Win32ThreadIDHelper : ThreadIDHelper
	{
		private readonly int currentProcessID;

		private readonly string currentProcessName;

		private readonly string currentProcessBaseName;

		/// <summary>
		/// Initializes a new instance of the <see cref="Win32ThreadIDHelper" /> class.
		/// </summary>
		public Win32ThreadIDHelper()
		{
			this.currentProcessID = NativeMethods.GetCurrentProcessId();

			var sb = new StringBuilder(512);
			if (0 == NativeMethods.GetModuleFileName(IntPtr.Zero, sb, sb.Capacity))
			{
				throw new InvalidOperationException("Cannot determine program name.");
			}

			this.currentProcessName = sb.ToString();
			this.currentProcessBaseName = Path.GetFileNameWithoutExtension(this.currentProcessName);
		}

		/// <summary>
		/// Gets current thread ID.
		/// </summary>
		/// <value></value>
		public override int CurrentThreadID
		{
			get { return Thread.CurrentThread.ManagedThreadId; }
		}

		/// <summary>
		/// Gets current process ID.
		/// </summary>
		/// <value></value>
		public override int CurrentProcessID
		{
			get { return this.currentProcessID; }
		}

		/// <summary>
		/// Gets current process name.
		/// </summary>
		/// <value></value>
		public override string CurrentProcessName
		{
			get { return this.currentProcessName; }
		}

		/// <summary>
		/// Gets current process name (excluding filename extension, if any).
		/// </summary>
		/// <value></value>
		public override string CurrentProcessBaseName
		{
			get { return this.currentProcessBaseName; }
		}
	}
}

#endif