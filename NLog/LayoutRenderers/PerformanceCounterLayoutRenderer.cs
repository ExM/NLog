using NLog.Common;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using NLog.Config;
using System;

namespace NLog.LayoutRenderers
{
	/// <summary>
	/// The performance counter.
	/// </summary>
	[LayoutRenderer("performancecounter")]
	public class PerformanceCounterLayoutRenderer : LayoutRenderer, ISupportsInitialize
	{
		private PerformanceCounter _perfCounter;

		/// <summary>
		/// Gets or sets the name of the counter category.
		/// </summary>
		/// <docgen category='Performance Counter Options' order='10' />
		[RequiredParameter]
		public string Category { get; set; }

		/// <summary>
		/// Gets or sets the name of the performance counter.
		/// </summary>
		/// <docgen category='Performance Counter Options' order='10' />
		[RequiredParameter]
		public string Counter { get; set; }

		/// <summary>
		/// Gets or sets the name of the performance counter instance (e.g. this.Global_).
		/// </summary>
		/// <docgen category='Performance Counter Options' order='10' />
		public string Instance { get; set; }

		/// <summary>
		/// Gets or sets the name of the machine to read the performance counter from.
		/// </summary>
		/// <docgen category='Performance Counter Options' order='10' />
		public string MachineName { get; set; }

		/// <summary>
		/// Renders the specified environment variable and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			if(!_isInitialized)
				throw new InvalidOperationException("required run Initialize method");
			builder.Append(_perfCounter.NextValue().ToString(CultureInfo.InvariantCulture));
		}

		private bool _isInitialized = false;

		public void Initialize(LoggingConfiguration configuration)
		{
			if(_isInitialized)
				return;
			
			_isInitialized = true;
			
			if(MachineName != null)
				_perfCounter = new PerformanceCounter(Category, Counter, Instance, MachineName);
			else
				_perfCounter = new PerformanceCounter(Category, Counter, Instance, true);
		}

		public void Close()
		{
			if(!_isInitialized)
				return;
			
			_isInitialized = false;
			
			_perfCounter.Close();
			_perfCounter = null;
		}
	}
}
