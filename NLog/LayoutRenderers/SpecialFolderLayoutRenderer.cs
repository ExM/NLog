

namespace NLog.LayoutRenderers
{
	using System;
	using System.IO;
	using System.Text;
	using NLog.Config;

	/// <summary>
	/// System special folder path (includes My Documents, My Music, Program Files, Desktop, and more).
	/// </summary>
	[LayoutRenderer("specialfolder")]
	[AppDomainFixedOutput]
	public class SpecialFolderLayoutRenderer : LayoutRenderer
	{
		/// <summary>
		/// Gets or sets the system special folder to use.
		/// </summary>
		/// <remarks>
		/// Full list of options is available at <a href="http://msdn2.microsoft.com/en-us/system.environment.specialfolder.aspx">MSDN</a>.
		/// The most common ones are:
		/// <ul>
		/// <li><b>ApplicationData</b> - roaming application data for current user.</li>
		/// <li><b>CommonApplicationData</b> - application data for all users.</li>
		/// <li><b>MyDocuments</b> - My Documents</li>
		/// <li><b>DesktopDirectory</b> - Desktop directory</li>
		/// <li><b>LocalApplicationData</b> - non roaming application data</li>
		/// <li><b>Personal</b> - user profile directory</li>
		/// <li><b>System</b> - System directory</li>
		/// </ul>
		/// </remarks>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultParameter]
		public Environment.SpecialFolder Folder { get; set; }

		/// <summary>
		/// Gets or sets the name of the file to be Path.Combine()'d with the directory name.
		/// </summary>
		/// <docgen category='Advanced Options' order='10' />
		public string File { get; set; }

		/// <summary>
		/// Gets or sets the name of the directory to be Path.Combine()'d with the directory name.
		/// </summary>
		/// <docgen category='Advanced Options' order='10' />
		public string Dir { get; set; }

		/// <summary>
		/// Renders the directory where NLog is located and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			string baseDir = Environment.GetFolderPath(this.Folder);

			if (this.File != null)
			{
				builder.Append(Path.Combine(baseDir, this.File));
			}
			else if (this.Dir != null)
			{
				builder.Append(Path.Combine(baseDir, this.Dir));
			}
			else
			{
				builder.Append(baseDir);
			}
		}
	}
}

