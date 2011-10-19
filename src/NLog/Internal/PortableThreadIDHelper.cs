
#if !SILVERLIGHT && !NET_CF

namespace NLog.Internal
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Threading;

	/// <summary>
	/// Portable implementation of <see cref="ThreadIDHelper"/>.
	/// </summary>
	internal class PortableThreadIDHelper : ThreadIDHelper
	{
		private const string UnknownProcessName = "<unknown>";

		private readonly int currentProcessID;

		private string currentProcessName;
		private string currentProcessBaseName;

		/// <summary>
		/// Initializes a new instance of the <see cref="PortableThreadIDHelper" /> class.
		/// </summary>
		public PortableThreadIDHelper()
		{
			this.currentProcessID = Process.GetCurrentProcess().Id;
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
			get
			{
				this.GetProcessName();
				return this.currentProcessName;
			}
		}

		/// <summary>
		/// Gets current process name (excluding filename extension, if any).
		/// </summary>
		/// <value></value>
		public override string CurrentProcessBaseName
		{
			get
			{
				this.GetProcessName();
				return this.currentProcessBaseName;
			}
		}

		/// <summary>
		/// Gets the name of the process.
		/// </summary>
		private void GetProcessName()
		{
			if (this.currentProcessName == null)
			{
				try
				{
					this.currentProcessName = Process.GetCurrentProcess().MainModule.FileName;
				}
				catch (Exception exception)
				{
					if (exception.MustBeRethrown())
					{
						throw;
					}

					this.currentProcessName = UnknownProcessName;
				}

				this.currentProcessBaseName = Path.GetFileNameWithoutExtension(this.currentProcessName);
			}
		}
	}
}

#endif