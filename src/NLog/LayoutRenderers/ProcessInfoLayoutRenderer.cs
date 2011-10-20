
#if !NET_CF && !MONO && !SILVERLIGHT

namespace NLog.LayoutRenderers
{
	using System;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Globalization;
	using System.Reflection;
	using System.Text;
	using NLog.Config;

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
			this.Property = ProcessInfoProperty.Id;
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
		protected override void InitializeLayoutRenderer()
		{
			base.InitializeLayoutRenderer();
			this.propertyInfo = typeof(Process).GetProperty(this.Property.ToString());
			if (this.propertyInfo == null)
			{
				throw new ArgumentException("Property '" + this.propertyInfo + "' not found in System.Diagnostics.Process");
			}

			this.process = Process.GetCurrentProcess();
		}

		/// <summary>
		/// Closes the layout renderer.
		/// </summary>
		protected override void CloseLayoutRenderer()
		{
			if (this.process != null)
			{
				this.process.Close();
				this.process = null;
			}

			base.CloseLayoutRenderer();
		}

		/// <summary>
		/// Renders the selected process information.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			if (this.propertyInfo != null)
			{
				builder.Append(Convert.ToString(this.propertyInfo.GetValue(this.process, null), CultureInfo.InvariantCulture));
			}
		}
	}
}

#endif
