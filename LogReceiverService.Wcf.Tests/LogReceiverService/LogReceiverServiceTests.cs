using System;
using System.IO;
//using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;
using NLog.Layouts;
using NLog.LogReceiverService;
using System.Runtime.Serialization;

namespace NLog.UnitTests.LogReceiverService
{
	[TestFixture]
	public class LogReceiverServiceTests : NLogTestBase
	{
		[Test]
		public void ToLogEventInfoTest()
		{
			var events = new NLogEvents
			{
				BaseTimeUtc = new DateTime(2010, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Ticks,
				ClientName = "foo",
				LayoutNames = new StringCollection { "foo", "bar", "baz" },
				Strings = new StringCollection { "logger1", "logger2", "logger3", "zzz", "message1" },
				Events =
					new[]
					{
						new NLogEvent
						{
							Id = 1,
							LevelOrdinal = 2,
							LoggerOrdinal = 0,
							TimeDelta = 30000000,
							MessageOrdinal = 4,
							Values = "0|1|2"
						},
						new NLogEvent
						{
							Id = 2,
							LevelOrdinal = 3,
							LoggerOrdinal = 2,
							MessageOrdinal = 4,
							TimeDelta = 30050000,
							Values = "0|1|3",
						}
					}
			};

			var converted = events.ToEventInfo();

			Assert.AreEqual(2, converted.Count);
			Assert.AreEqual("message1", converted[0].FormattedMessage);
			Assert.AreEqual("message1", converted[1].FormattedMessage);

			Assert.AreEqual(new DateTime(2010, 1, 1, 0, 0, 3, 0, DateTimeKind.Utc), converted[0].TimeStamp.ToUniversalTime());
			Assert.AreEqual(new DateTime(2010, 1, 1, 0, 0, 3, 5, DateTimeKind.Utc), converted[1].TimeStamp.ToUniversalTime());

			Assert.AreEqual("logger1", converted[0].LoggerName);
			Assert.AreEqual("logger3", converted[1].LoggerName);

			Assert.AreEqual(LogLevel.Info, converted[0].Level);
			Assert.AreEqual(LogLevel.Warn, converted[1].Level);

			Layout fooLayout = "${event-context:foo}";
			Layout barLayout = "${event-context:bar}";
			Layout bazLayout = "${event-context:baz}";

			fooLayout.Initialize(CommonCfg);
			Assert.AreEqual("logger1", fooLayout.Render(converted[0]));
			Assert.AreEqual("logger1", fooLayout.Render(converted[1]));

			barLayout.Initialize(CommonCfg);
			Assert.AreEqual("logger2", barLayout.Render(converted[0]));
			Assert.AreEqual("logger2", barLayout.Render(converted[1]));

			bazLayout.Initialize(CommonCfg);
			Assert.AreEqual("logger3", bazLayout.Render(converted[0]));
			Assert.AreEqual("zzz", bazLayout.Render(converted[1]));
		}

		[Test]
		public void NoLayoutsTest()
		{
			var events = new NLogEvents
			{
				BaseTimeUtc = new DateTime(2010, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Ticks,
				ClientName = "foo",
				LayoutNames = new StringCollection(),
				Strings = new StringCollection { "logger1", "logger2", "logger3", "zzz", "message1" },
				Events =
					new[]
					{
						new NLogEvent
						{
							Id = 1,
							LevelOrdinal = 2,
							LoggerOrdinal = 0,
							TimeDelta = 30000000,
							MessageOrdinal = 4,
							Values = null,
						},
						new NLogEvent
						{
							Id = 2,
							LevelOrdinal = 3,
							LoggerOrdinal = 2,
							MessageOrdinal = 4,
							TimeDelta = 30050000,
							Values = null,
						}
					}
			};

			var converted = events.ToEventInfo();

			Assert.AreEqual(2, converted.Count);
			Assert.AreEqual("message1", converted[0].FormattedMessage);
			Assert.AreEqual("message1", converted[1].FormattedMessage);

			Assert.AreEqual(new DateTime(2010, 1, 1, 0, 0, 3, 0, DateTimeKind.Utc), converted[0].TimeStamp.ToUniversalTime());
			Assert.AreEqual(new DateTime(2010, 1, 1, 0, 0, 3, 5, DateTimeKind.Utc), converted[1].TimeStamp.ToUniversalTime());

			Assert.AreEqual("logger1", converted[0].LoggerName);
			Assert.AreEqual("logger3", converted[1].LoggerName);

			Assert.AreEqual(LogLevel.Info, converted[0].Level);
			Assert.AreEqual(LogLevel.Warn, converted[1].Level);
		}

		/// <summary>
		/// Ensures that serialization formats of DataContractSerializer and XmlSerializer are the same
		/// on the same <see cref="NLogEvents"/> object.
		/// </summary>
		[Test]
		public void CompareSerializationFormats()
		{
			var events = new NLogEvents
			{
				BaseTimeUtc = DateTime.UtcNow.Ticks,
				ClientName = "foo",
				LayoutNames = new StringCollection { "foo", "bar", "baz" },
				Strings = new StringCollection { "logger1", "logger2", "logger3" },
				Events =
					new[]
					{
						new NLogEvent
						{
							Id = 1,
							LevelOrdinal = 2,
							LoggerOrdinal = 0,
							TimeDelta = 34,
							Values = "1|2|3"
						},
						new NLogEvent
						{
							Id = 2,
							LevelOrdinal = 3,
							LoggerOrdinal = 2,
							TimeDelta = 345,
							Values = "1|2|3",
						}
					}
			};

			var serializer1 = new XmlSerializer(typeof(NLogEvents));
			var sw1 = new StringWriter();
			using (var writer1 = XmlWriter.Create(sw1, new XmlWriterSettings { Indent = true }))
			{
				var namespaces = new XmlSerializerNamespaces();
				namespaces.Add("i", "http://www.w3.org/2001/XMLSchema-instance");

				serializer1.Serialize(writer1, events, namespaces);
			}

			var serializer2 = new DataContractSerializer(typeof(NLogEvents));
			var sw2 = new StringWriter();
			using (var writer2 = XmlWriter.Create(sw2, new XmlWriterSettings { Indent = true }))
			{
				serializer2.WriteObject(writer2, events);
			}

			var xml1 = sw1.ToString();
			var xml2 = sw2.ToString();

			Assert.AreEqual(xml1, xml2);
		}
	}
}
