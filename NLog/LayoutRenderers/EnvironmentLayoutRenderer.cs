using System.Text;
using NLog.Config;
using NLog.Internal;
using System;
using System.Security;

namespace NLog.LayoutRenderers
{
	/// <summary>
	/// The environment variable.
	/// </summary>
	[LayoutRenderer("environment")]
	public class EnvironmentLayoutRenderer : LayoutRenderer
	{
		/// <summary>
		/// Gets or sets the name of the environment variable.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[RequiredParameter]
		[DefaultParameter]
		public string Variable { get; set; }

		/// <summary>
		/// Renders the specified environment variable and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			if (Variable != null)
				builder.Append(GetSafeEnvironmentVariable(Variable));
		}

		private static string GetSafeEnvironmentVariable(string name)
		{
			try
			{
				string s = Environment.GetEnvironmentVariable(name);
				if (s == null || s.Length == 0)
					return null;
				return s;
			}
			catch (SecurityException)
			{
				return string.Empty;
			}
		}
	}
}

