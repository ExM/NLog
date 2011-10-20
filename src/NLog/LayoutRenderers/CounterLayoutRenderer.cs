
namespace NLog.LayoutRenderers
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Globalization;
	using System.Text;

	/// <summary>
	/// A counter value (increases on each layout rendering).
	/// </summary>
	[LayoutRenderer("counter")]
	public class CounterLayoutRenderer : LayoutRenderer
	{
		private static Dictionary<string, int> sequences = new Dictionary<string, int>();

		/// <summary>
		/// Initializes a new instance of the <see cref="CounterLayoutRenderer" /> class.
		/// </summary>
		public CounterLayoutRenderer()
		{
			this.Increment = 1;
			this.Value = 1;
		}

		/// <summary>
		/// Gets or sets the initial value of the counter.
		/// </summary>
		/// <docgen category='Counter Options' order='10' />
		[DefaultValue(1)]
		public int Value { get; set; }

		/// <summary>
		/// Gets or sets the value to be added to the counter after each layout rendering.
		/// </summary>
		/// <docgen category='Counter Options' order='10' />
		[DefaultValue(1)]
		public int Increment { get; set; }

		/// <summary>
		/// Gets or sets the name of the sequence. Different named sequences can have individual values.
		/// </summary>
		/// <docgen category='Counter Options' order='10' />
		public string Sequence { get; set; }

		/// <summary>
		/// Renders the specified counter value and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			int v;

			if (this.Sequence != null)
			{
				v = GetNextSequenceValue(this.Sequence, this.Value, this.Increment);
			}
			else
			{
				v = this.Value;
				this.Value += this.Increment;
			}

			builder.Append(v.ToString(CultureInfo.InvariantCulture));
		}

		private static int GetNextSequenceValue(string sequenceName, int defaultValue, int increment)
		{
			lock (sequences)
			{
				int val;

				if (!sequences.TryGetValue(sequenceName, out val))
				{
					val = defaultValue;
				}

				int retVal = val;

				val += increment;
				sequences[sequenceName] = val;
				return retVal;
			}
		}
	}
}
