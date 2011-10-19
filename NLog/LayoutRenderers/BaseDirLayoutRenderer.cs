
#if !SILVERLIGHT

namespace NLog.LayoutRenderers
{
	using System;
	using System.IO;
	using System.Text;

	using NLog.Config;

	/// <summary>
	/// The current application domain's base directory.
	/// </summary>
	[LayoutRenderer("basedir")]
	[AppDomainFixedOutput]
	public class BaseDirLayoutRenderer : LayoutRenderer
	{
		private string baseDir;

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseDirLayoutRenderer" /> class.
		/// </summary>
		public BaseDirLayoutRenderer()
		{
#if !NET_CF
			this.baseDir = AppDomain.CurrentDomain.BaseDirectory;
#else
			this.baseDir = NLog.Internal.CompactFrameworkHelper.GetExeBaseDir();
#endif
		}

		/// <summary>
		/// Gets or sets the name of the file to be Path.Combine()'d with with the base directory.
		/// </summary>
		/// <docgen category='Advanced Options' order='10' />
		public string File { get; set; }

		/// <summary>
		/// Gets or sets the name of the directory to be Path.Combine()'d with with the base directory.
		/// </summary>
		/// <docgen category='Advanced Options' order='10' />
		public string Dir { get; set; }

		/// <summary>
		/// Renders the application base directory and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			if (this.File != null)
			{
				builder.Append(Path.Combine(this.baseDir, this.File));
			}
			else if (this.Dir != null)
			{
				builder.Append(Path.Combine(this.baseDir, this.Dir));
			}
			else
			{
				builder.Append(this.baseDir);
			}
		}
	}
}

#endif
