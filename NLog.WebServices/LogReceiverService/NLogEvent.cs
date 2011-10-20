
namespace NLog.LogReceiverService
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
#if WCF_SUPPORTED
	using System.Runtime.Serialization;
#endif
	using System.Text;
	using System.Xml.Serialization;

	/// <summary>
	/// Wire format for NLog Event.
	/// </summary>
#if WCF_SUPPORTED
	[DataContract(Name = "e", Namespace = LogReceiverServiceConfig.WebServiceNamespace)]
#endif
	[XmlType(Namespace = LogReceiverServiceConfig.WebServiceNamespace)]
	[DebuggerDisplay("Event ID = {Id} Level={LevelName} Values={Values.Count}")]
	public class NLogEvent
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NLogEvent"/> class.
		/// </summary>
		public NLogEvent()
		{
			this.ValueIndexes = new List<int>();
		}

		/// <summary>
		/// Gets or sets the client-generated identifier of the event.
		/// </summary>
#if WCF_SUPPORTED
		[DataMember(Name = "id", Order = 0)]
#endif
		[XmlElement("id", Order = 0)]
		public int Id { get; set; }

		/// <summary>
		/// Gets or sets the ordinal of the log level.
		/// </summary>
#if WCF_SUPPORTED
		[DataMember(Name = "lv", Order = 1)]
#endif
		[XmlElement("lv", Order = 1)]
		public int LevelOrdinal { get; set; }

		/// <summary>
		/// Gets or sets the logger ordinal (index into <see cref="NLogEvents.Strings"/>.
		/// </summary>
		/// <value>The logger ordinal.</value>
#if WCF_SUPPORTED
		[DataMember(Name = "lg", Order = 2)]
#endif
		[XmlElement("lg", Order = 2)]
		public int LoggerOrdinal { get; set; }

		/// <summary>
		/// Gets or sets the time delta (in ticks) between the time of the event and base time.
		/// </summary>
#if WCF_SUPPORTED
		[DataMember(Name = "ts", Order = 3)]
#endif
		[XmlElement("ts", Order = 3)]
		public long TimeDelta { get; set; }

		/// <summary>
		/// Gets or sets the message string index.
		/// </summary>
#if WCF_SUPPORTED
		[DataMember(Name = "m", Order = 4)]
#endif
		[XmlElement("m", Order = 4)]
		public int MessageOrdinal { get; set; }

		/// <summary>
		/// Gets or sets the collection of layout values.
		/// </summary>
#if WCF_SUPPORTED
		[DataMember(Name = "val", Order = 100)]
#endif
		[XmlElement("val", Order = 100)]
		public string Values
		{
			get
			{
				var sb = new StringBuilder();
				string separator = string.Empty;

				if (this.ValueIndexes != null)
				{
					foreach (int index in this.ValueIndexes)
					{
						sb.Append(separator);
						sb.Append(index);
						separator = "|";
					}
				}

				return sb.ToString();
			}

			set
			{
				if (this.ValueIndexes != null)
				{
					this.ValueIndexes.Clear();
				}
				else
				{
					this.ValueIndexes = new List<int>();
				}

				if (!string.IsNullOrEmpty(value))
				{
					string[] chunks = value.Split('|');

					foreach (string chunk in chunks)
					{
						this.ValueIndexes.Add(Convert.ToInt32(chunk, CultureInfo.InvariantCulture));
					}
				}
			}
		}

		/// <summary>
		/// Gets the collection of indexes into <see cref="NLogEvents.Strings"/> array for each layout value.
		/// </summary>
#if WCF_SUPPORTED
		[IgnoreDataMember]
#endif
		[XmlIgnore]
		public IList<int> ValueIndexes { get; private set; }

		/// <summary>
		/// Converts the <see cref="NLogEvent"/> to <see cref="LogEventInfo"/>.
		/// </summary>
		/// <param name="context">The <see cref="NLogEvent"/> object this <see cref="NLogEvent" /> is part of..</param>
		/// <param name="loggerNamePrefix">The logger name prefix to prepend in front of the logger name.</param>
		/// <returns>Converted <see cref="LogEventInfo"/>.</returns>
		internal LogEventInfo ToEventInfo(NLogEvents context, string loggerNamePrefix)
		{
			var result = new LogEventInfo(LogLevel.FromOrdinal(this.LevelOrdinal), loggerNamePrefix + context.Strings[this.LoggerOrdinal], context.Strings[this.MessageOrdinal]);
			result.TimeStamp = new DateTime(context.BaseTimeUtc + this.TimeDelta, DateTimeKind.Utc).ToLocalTime();
			for (int i = 0; i < context.LayoutNames.Count; ++i)
			{
				string layoutName = context.LayoutNames[i];
				string layoutValue = context.Strings[this.ValueIndexes[i]];

				result.Properties[layoutName] = layoutValue;
			}

			return result;
		}
	}
}