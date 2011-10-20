
namespace NLog.LayoutRenderers
{
	using System.IO;
	using System.Text;

	using NLog.Config;

	/// <summary>
	/// A temporary directory.
	/// </summary>
	[LayoutRenderer("tempdir")]
	[AppDomainFixedOutput]
	public class TempDirLayoutRenderer : LayoutRenderer
	{
		private static string tempDir = Path.GetTempPath();

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
			string baseDir = tempDir;

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