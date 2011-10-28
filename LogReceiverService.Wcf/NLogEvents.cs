using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;

namespace NLog.LogReceiverService
{
	/// <summary>
	/// Wire format for NLog event package.
	/// </summary>
	[DataContract(Name = "events", Namespace = LogReceiverServiceConfig.WebServiceNamespace)]
	[XmlType(Namespace = LogReceiverServiceConfig.WebServiceNamespace)]
	[XmlRoot("events", Namespace = LogReceiverServiceConfig.WebServiceNamespace)]
	[DebuggerDisplay("Count = {Events.Length}")]
	public class NLogEvents
	{
		/// <summary>
		/// Gets or sets the name of the client.
		/// </summary>
		/// <value>The name of the client.</value>
		[DataMember(Name = "cli", Order = 0)]
		[XmlElement("cli", Order = 0)]
		public string ClientName { get; set; }

		/// <summary>
		/// Gets or sets the base time (UTC ticks) for all events in the package.
		/// </summary>
		/// <value>The base time UTC.</value>
		[DataMember(Name = "bts", Order = 1)]
		[XmlElement("bts", Order = 1)]
		public long BaseTimeUtc { get; set; }

		/// <summary>
		/// Gets or sets the collection of layout names which are shared among all events.
		/// </summary>
		/// <value>The layout names.</value>
		[DataMember(Name = "lts", Order = 100)]
		[XmlArray("lts", Order = 100)]
		[XmlArrayItem("l")]
		public StringCollection LayoutNames { get; set; }

		/// <summary>
		/// Gets or sets the collection of logger names.
		/// </summary>
		/// <value>The logger names.</value>
		[DataMember(Name = "str", Order = 200)]
		[XmlArray("str", Order = 200)]
		[XmlArrayItem("l")]
		public StringCollection Strings { get; set; }

		/// <summary>
		/// Gets or sets the list of events.
		/// </summary>
		/// <value>The events.</value>
		[DataMember(Name = "ev", Order = 1000)]
		[XmlArray("ev", Order = 1000)]
		[XmlArrayItem("e")]
		public NLogEvent[] Events { get; set; }

		/// <summary>
		/// Converts the events to sequence of <see cref="LogEventInfo"/> objects suitable for routing through NLog.
		/// </summary>
		/// <param name="loggerNamePrefix">The logger name prefix to prepend in front of each logger name.</param>
		/// <returns>
		/// Sequence of <see cref="LogEventInfo"/> objects.
		/// </returns>
		public IList<LogEventInfo> ToEventInfo(string loggerNamePrefix)
		{
			var result = new LogEventInfo[Events.Length];

			for (int i = 0; i < result.Length; ++i)
			{
				result[i] = Events[i].ToEventInfo(this, loggerNamePrefix);
			}

			return result;
		}

		/// <summary>
		/// Converts the events to sequence of <see cref="LogEventInfo"/> objects suitable for routing through NLog.
		/// </summary>
		/// <returns>
		/// Sequence of <see cref="LogEventInfo"/> objects.
		/// </returns>
		public IList<LogEventInfo> ToEventInfo()
		{
			return ToEventInfo(string.Empty);
		}
	}
}
