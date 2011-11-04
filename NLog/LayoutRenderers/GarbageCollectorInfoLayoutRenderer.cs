using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace NLog.LayoutRenderers
{
	/// <summary>
	/// The information about the garbage collector.
	/// </summary>
	[LayoutRenderer("gc")]
	public class GarbageCollectorInfoLayoutRenderer : LayoutRenderer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GarbageCollectorInfoLayoutRenderer" /> class.
		/// </summary>
		public GarbageCollectorInfoLayoutRenderer()
		{
			Property = GarbageCollectorProperty.TotalMemory;
		}

		/// <summary>
		/// Gets or sets the property to retrieve.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue("TotalMemory")]
		public GarbageCollectorProperty Property { get; set; }
		
		/// <summary>
		/// Renders the selected process information.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			builder.Append(GetValue());
		}

		private string GetValue()
		{
			switch (Property)
			{
				case GarbageCollectorProperty.TotalMemory:
					return GC.GetTotalMemory(false).ToString(CultureInfo.InvariantCulture);
			
				case GarbageCollectorProperty.TotalMemoryForceCollection:
					return GC.GetTotalMemory(true).ToString(CultureInfo.InvariantCulture);
			
				case GarbageCollectorProperty.CollectionCount0:
					return GC.CollectionCount(0).ToString(CultureInfo.InvariantCulture);
			
				case GarbageCollectorProperty.CollectionCount1:
					return GC.CollectionCount(1).ToString(CultureInfo.InvariantCulture);
			
				case GarbageCollectorProperty.CollectionCount2:
					return GC.CollectionCount(2).ToString(CultureInfo.InvariantCulture);
				
				case GarbageCollectorProperty.MaxGeneration:
					return GC.MaxGeneration.ToString(CultureInfo.InvariantCulture);
					
				default:
					throw new ArgumentException("Property '" + Property + "' not found");
			}
		}
	}
}
