
namespace NLog.LogReceiverService
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;

#if WCF_SUPPORTED
	using System.Runtime.Serialization;
	using System.ServiceModel;
#endif
	using System.Xml.Serialization;

	/// <summary>
	/// Wire format for NLog event package.
	/// </summary>
#if WCF_SUPPORTED
	[DataContract(Name = "events", Namespace = LogReceiverServiceConfig.WebServiceNamespace)]
#endif
	[XmlType(Namespace = LogReceiverServiceConfig.WebServiceNamespace)]
	[XmlRoot("events", Namespace = LogReceiverServiceConfig.WebServiceNamespace)]
	[DebuggerDisplay("Count = {Events.Length}")]
	public class NLogEvents
	{
		/// <summary>
		/// Gets or sets the name of the client.
		/// </summary>
		/// <value>The name of the client.</value>
#if WCF_SUPPORTED
		[DataMember(Name = "cli", Order = 0)]
#endif
		[XmlElement("cli", Order = 0)]
		public string ClientName { get; set; }

		/// <summary>
		/// Gets or sets the base time (UTC ticks) for all events in the package.
		/// </summary>
		/// <value>The base time UTC.</value>
#if WCF_SUPPORTED
		[DataMember(Name = "bts", Order = 1)]
#endif
		[XmlElement("bts", Order = 1)]
		public long BaseTimeUtc { get; set; }

		/// <summary>
		/// Gets or sets the collection of layout names which are shared among all events.
		/// </summary>
		/// <value>The layout names.</value>
#if WCF_SUPPORTED
		[DataMember(Name = "lts", Order = 100)]
#endif
		[XmlArray("lts", Order = 100)]
		[XmlArrayItem("l")]
		public StringCollection LayoutNames { get; set; }

		/// <summary>
		/// Gets or sets the collection of logger names.
		/// </summary>
		/// <value>The logger names.</value>
#if WCF_SUPPORTED
		[DataMember(Name = "str", Order = 200)]
#endif
		[XmlArray("str", Order = 200)]
		[XmlArrayItem("l")]
		public StringCollection Strings { get; set; }

		/// <summary>
		/// Gets or sets the list of events.
		/// </summary>
		/// <value>The events.</value>
#if WCF_SUPPORTED
		[DataMember(Name = "ev", Order = 1000)]
#endif
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
			var result = new LogEventInfo[this.Events.Length];

			for (int i = 0; i < result.Length; ++i)
			{
				result[i] = this.Events[i].ToEventInfo(this, loggerNamePrefix);
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
			return this.ToEventInfo(string.Empty);
		}
	}
}
