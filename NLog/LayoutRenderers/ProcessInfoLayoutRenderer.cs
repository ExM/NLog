using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using NLog.Config;
using NLog.Common;

namespace NLog.LayoutRenderers
{
	/// <summary>
	/// The information about the running process.
	/// </summary>
	[LayoutRenderer("processinfo")]
	public class ProcessInfoLayoutRenderer : LayoutRenderer, ISupportsInitialize
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
		/// Renders the selected process information.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			if(!_isInitialized)
				throw new InvalidOperationException("required run Initialize method");
			if (propertyInfo != null)
				builder.Append(Convert.ToString(propertyInfo.GetValue(process, null), CultureInfo.InvariantCulture));
		}
		
		private bool _isInitialized = false;

		public void Initialize(LoggingConfiguration configuration)
		{
			if(_isInitialized)
				return;
			
			_isInitialized = true;
			
			propertyInfo = typeof(Process).GetProperty(Property.ToString());
			if(propertyInfo == null)
				throw new ArgumentException("Property '" + propertyInfo + "' not found in System.Diagnostics.Process");

			process = Process.GetCurrentProcess();
		}

		public void Close()
		{
			if(!_isInitialized)
				return;
			
			_isInitialized = false;
			
			process.Close();
			process = null;
		}
	}
}

