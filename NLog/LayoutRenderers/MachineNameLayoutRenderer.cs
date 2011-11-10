
namespace NLog.LayoutRenderers
{
	using System;
	using System.Text;
	using NLog.Common;
	using NLog.Config;
	using NLog.Internal;

	/// <summary>
	/// The machine name that the process is running on.
	/// </summary>
	[LayoutRenderer("machinename")]
	[AppDomainFixedOutput]
	[ThreadAgnostic]
	public class MachineNameLayoutRenderer : LayoutRenderer
	{
		internal string MachineName { get; private set; }

		/// <summary>
		/// Initializes the layout renderer.
		/// </summary>
		protected override void InternalInit(LoggingConfiguration cfg)
		{
			base.InternalInit(cfg);
			try
			{
				this.MachineName = Environment.MachineName;
			}
			catch (Exception exception)
			{
				if (exception.MustBeRethrown())
				{
					throw;
				}

				InternalLogger.Error("Error getting machine name {0}", exception);
				this.MachineName = string.Empty;
			}
		}

		/// <summary>
		/// Renders the machine name and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			builder.Append(this.MachineName);
		}
	}
}
