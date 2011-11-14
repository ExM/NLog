using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using NLog.Internal;
using NLog.Config;
using NLog.Common;

namespace NLog.LayoutRenderers
{
	/// <summary>
	/// High precision timer, based on the value returned from QueryPerformanceCounter() optionally converted to seconds.
	/// </summary>
	[LayoutRenderer("qpc")]
	public class QueryPerformanceCounterLayoutRenderer : LayoutRenderer, ISupportsInitialize
	{
		private bool _raw;
		private ulong _firstQpcValue;
		private ulong _lastQpcValue;
		private double _frequency = 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="QueryPerformanceCounterLayoutRenderer" /> class.
		/// </summary>
		public QueryPerformanceCounterLayoutRenderer()
		{
			Normalize = true;
			Difference = false;
			Precision = 4;
			AlignDecimalPoint = true;
		}

		/// <summary>
		/// Gets or sets a value indicating whether to normalize the result by subtracting 
		/// it from the result of the first call (so that it's effectively zero-based).
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(true)]
		public bool Normalize { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to output the difference between the result 
		/// of QueryPerformanceCounter and the previous one.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(false)]
		public bool Difference { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to convert the result to seconds by dividing 
		/// by the result of QueryPerformanceFrequency().
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(true)]
		public bool Seconds
		{
			get
			{
				return !_raw;
			}
			set
			{
				_raw = !value;
			}
		}

		/// <summary>
		/// Gets or sets the number of decimal digits to be included in output.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(4)]
		public int Precision { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to align decimal point (emit non-significant zeros).
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(true)]
		public bool AlignDecimalPoint { get; set; }

		/// <summary>
		/// Renders the ticks value of current time and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			if(!_isInitialized)
				throw new InvalidOperationException("required run Initialize method");
				
			ulong qpcValue;

			if (!NativeMethods.QueryPerformanceCounter(out qpcValue))
				return;

			ulong v = qpcValue;

			if (Difference)
				qpcValue -= _lastQpcValue;
			else if (Normalize)
				qpcValue -= _firstQpcValue;

			_lastQpcValue = v;

			string stringValue;

			if (Seconds)
			{
				double val = Math.Round(qpcValue / _frequency, Precision);

				stringValue = Convert.ToString(val, CultureInfo.InvariantCulture);
				if (AlignDecimalPoint)
				{
					int p = stringValue.IndexOf('.');
					if (p == -1)
						stringValue += "." + new string('0', Precision);
					else
						stringValue += new string('0', Precision - (stringValue.Length - 1 - p));
				}
			}
			else
			{
				stringValue = Convert.ToString(qpcValue, CultureInfo.InvariantCulture);
			}

			builder.Append(stringValue);
		}
		
		private bool _isInitialized = false;

		public void Initialize(LoggingConfiguration configuration)
		{
			if(_isInitialized)
				return;
			
			_isInitialized = true;
			
			ulong performanceFrequency;

			if(!NativeMethods.QueryPerformanceFrequency(out performanceFrequency))
				throw new InvalidOperationException("Cannot determine high-performance counter frequency.");

			ulong qpcValue;

			if(!NativeMethods.QueryPerformanceCounter(out qpcValue))
				throw new InvalidOperationException("Cannot determine high-performance counter value.");

			_frequency = performanceFrequency;
			_firstQpcValue = qpcValue;
			_lastQpcValue = qpcValue;
		}

		public void Close()
		{
			if(!_isInitialized)
				return;
			
			_isInitialized = false;
		}
	}
}
