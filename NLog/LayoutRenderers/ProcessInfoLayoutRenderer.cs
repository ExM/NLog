using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using NLog.Config;

namespace NLog.LayoutRenderers
{
	/// <summary>
	/// The information about the running process.
	/// </summary>
	[LayoutRenderer("processinfo")]
	public class ProcessInfoLayoutRenderer : LayoutRenderer
	{
		private Process process;

		private PropertyInfo propertyInfo;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProcessInfoLayoutRenderer" /> class.
		/// </summary>
		public ProcessInfoLayoutRenderer()
		{
			Property = ProcessInfoProperty.Id;
		}

		/// <summary>
		/// Gets or sets the property to retrieve.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue("Id"), DefaultParameter]
		public ProcessInfoProperty Property { get; set; }

		/// <summary>
		/// Initializes the layout renderer.
		/// </summary>
		protected override void InternalInit(LoggingConfiguration cfg)
		{
			base.InternalInit(cfg);
			propertyInfo = typeof(Process).GetProperty(Property.ToString());
			if (propertyInfo == null)
			{
				throw new ArgumentException("Property '" + propertyInfo + "' not found in System.Diagnostics.Process");
			}

			process = Process.GetCurrentProcess();
		}

		/// <summary>
		/// Closes the layout renderer.
		/// </summary>
		protected override void InternalClose()
		{
			base.InternalClose();
		
			if (process != null)
			{
				process.Close();
				process = null;
			}
		}

		/// <summary>
		/// Renders the selected process information.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			if (propertyInfo != null)
			{
				builder.Append(Convert.ToString(propertyInfo.GetValue(process, null), CultureInfo.InvariantCulture));
			}
		}
	}
}

