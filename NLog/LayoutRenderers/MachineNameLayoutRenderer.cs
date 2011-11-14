using System;
using System.Text;
using NLog.Common;
using NLog.Config;
using NLog.Internal;
	
namespace NLog.LayoutRenderers
{
	/// <summary>
	/// The machine name that the process is running on.
	/// </summary>
	[LayoutRenderer("machinename")]
	[AppDomainFixedOutput]
	[ThreadAgnostic]
	public sealed class MachineNameLayoutRenderer : LayoutRenderer, ISupportsInitialize
	{
		internal string MachineName { get; private set; }

		/// <summary>
		/// Renders the machine name and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			if(!_isInitialized)
				throw new InvalidOperationException("required run Initialize method");
			builder.Append(MachineName);
		}

		private bool _isInitialized = false;

		public void Initialize(LoggingConfiguration configuration)
		{
			if(_isInitialized)
				return;
			
			_isInitialized = true;
			
			try
			{
				MachineName = Environment.MachineName;
			}
			catch(Exception exception)
			{
				if(exception.MustBeRethrown())
					throw;

				InternalLogger.Error("Error getting machine name {0}", exception);
				MachineName = string.Empty;
			}
		}

		public void Close()
		{
			if(!_isInitialized)
				return;
			
			_isInitialized = false;
		}
	}
}
