using System;
using System.Globalization;
using NLog.Config;
using NLog.Layouts;

namespace NLog.Targets
{
	/// <summary>
	/// A parameter to MethodCall.
	/// </summary>
	[NLogConfigurationItem]
	public class MethodCallParameter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MethodCallParameter" /> class.
		/// </summary>
		public MethodCallParameter()
		{
			this.Type = typeof(string);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MethodCallParameter" /> class.
		/// </summary>
		/// <param name="layout">The layout to use for parameter value.</param>
		public MethodCallParameter(Layout layout)
		{
			this.Type = typeof(string);
			this.Layout = layout;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MethodCallParameter" /> class.
		/// </summary>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="layout">The layout.</param>
		public MethodCallParameter(string parameterName, Layout layout)
		{
			this.Type = typeof(string);
			this.Name = parameterName;
			this.Layout = layout;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MethodCallParameter" /> class.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="layout">The layout.</param>
		/// <param name="type">The type of the parameter.</param>
		public MethodCallParameter(string name, Layout layout, Type type)
		{
			this.Type = type;
			this.Name = name;
			this.Layout = layout;
		}

		/// <summary>
		/// Gets or sets the name of the parameter.
		/// </summary>
		/// <docgen category='Parameter Options' order='10' />
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the type of the parameter.
		/// </summary>
		/// <docgen category='Parameter Options' order='10' />
		public Type Type { get; set; }

		/// <summary>
		/// Gets or sets the layout that should be use to calcuate the value for the parameter.
		/// </summary>
		/// <docgen category='Parameter Options' order='10' />
		[RequiredParameter]
		public Layout Layout { get; set; }

		internal object GetValue(LogEventInfo logEvent)
		{
			return Convert.ChangeType(this.Layout.Render(logEvent), this.Type, CultureInfo.InvariantCulture);
		}
	}
}
