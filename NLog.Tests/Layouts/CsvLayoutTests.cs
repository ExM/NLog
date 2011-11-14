using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using NLog.Layouts;
using NLog.Config;
using NLog.Common;

namespace NLog.UnitTests.Layouts
{
	[TestFixture]
	public class CsvLayoutTests : NLogTestBase
	{
		[Test]
		public void EndToEndTest()
		{
			File.Delete("CSVLayoutEndToEnd1.txt");
			Assert.IsFalse(File.Exists("CSVLayoutEndToEnd1.txt"));

			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets>
				  <target name='f' type='File' fileName='CSVLayoutEndToEnd1.txt'>
					<layout type='CSVLayout'>
					  <Delimiter>Comma</Delimiter>
					  <column name='level' layout='${level}' />
					  <column name='message' layout='${message}' />
					  <column name='counter' layout='${counter}' />
					</layout>
				  </target>
				</targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='f' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("msg");
			logger.Info("msg2");
			logger.Warn("Message with, a comma");

			using (StreamReader sr = File.OpenText("CSVLayoutEndToEnd1.txt"))
			{
				Assert.AreEqual("level,message,counter", sr.ReadLine());
				Assert.AreEqual("Debug,msg,1", sr.ReadLine());
				Assert.AreEqual("Info,msg2,2", sr.ReadLine());
				Assert.AreEqual("Warn,\"Message with, a comma\",3", sr.ReadLine());
			}
		}

		[Test]
		public void NoHeadersTest()
		{
			File.Delete("CSVLayoutEndToEnd2.txt");
			Assert.IsFalse(File.Exists("CSVLayoutEndToEnd2.txt"));

			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets>
				  <target name='f' type='File' fileName='CSVLayoutEndToEnd2.txt'>
					<layout type='CSVLayout' withHeader='false'>
					  <Delimiter>Comma</Delimiter>
					  <column name='level' layout='${level}' />
					  <column name='message' layout='${message}' />
					  <column name='counter' layout='${counter}' />
					</layout>
				  </target>
				</targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='f' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("msg");
			logger.Info("msg2");
			logger.Warn("Message with, a comma");

			using (StreamReader sr = File.OpenText("CSVLayoutEndToEnd2.txt"))
			{
				Assert.AreEqual("Debug,msg,1", sr.ReadLine());
				Assert.AreEqual("Info,msg2,2", sr.ReadLine());
				Assert.AreEqual("Warn,\"Message with, a comma\",3", sr.ReadLine());
			}
		}

		[Test]
		public void CsvLayoutRenderingNoQuoting()
		{
			var delimiters = new Dictionary<CsvColumnDelimiterMode, string>
			{
				{ CsvColumnDelimiterMode.Auto, CultureInfo.CurrentCulture.TextInfo.ListSeparator },
				{ CsvColumnDelimiterMode.Comma, "," },
				{ CsvColumnDelimiterMode.Semicolon, ";" },
				{ CsvColumnDelimiterMode.Space, " " },
				{ CsvColumnDelimiterMode.Tab, "\t" },
				{ CsvColumnDelimiterMode.Pipe, "|" },
				{ CsvColumnDelimiterMode.Custom, "zzz" },
			};

			foreach (var delim in delimiters)
			{
				var csvLayout = new CsvLayout()
				{
					Quoting = CsvQuotingMode.Nothing,
					Columns =
						{
							new CsvColumn("date", "${longdate}"),
							new CsvColumn("level", "${level}"),
							new CsvColumn("message;text", "${message}"),
						},
					Delimiter = delim.Key,
					CustomColumnDelimiter = "zzz",
				};

				var ev = new LogEventInfo();
				ev.TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56);
				ev.Level = LogLevel.Info;
				ev.Message = "hello, world";

				string sep = delim.Value;
				csvLayout.DeepInitialize(CommonCfg);

				Assert.AreEqual("2010-01-01 12:34:56.0000" + sep + "Info" + sep + "hello, world", csvLayout.Render(ev));
				Assert.AreEqual("date" + sep + "level" + sep + "message;text", csvLayout.Header.Render(ev));
			}
		}

		[Test]
		public void CsvLayoutRenderingFullQuoting()
		{
			var delimiters = new Dictionary<CsvColumnDelimiterMode, string>
			{
				{ CsvColumnDelimiterMode.Auto, CultureInfo.CurrentCulture.TextInfo.ListSeparator },
				{ CsvColumnDelimiterMode.Comma, "," },
				{ CsvColumnDelimiterMode.Semicolon, ";" },
				{ CsvColumnDelimiterMode.Space, " " },
				{ CsvColumnDelimiterMode.Tab, "\t" },
				{ CsvColumnDelimiterMode.Pipe, "|" },
				{ CsvColumnDelimiterMode.Custom, "zzz" },
			};

			foreach (var delim in delimiters)
			{
				var csvLayout = new CsvLayout()
				{
					Quoting = CsvQuotingMode.All,
					Columns =
						{
							new CsvColumn("date", "${longdate}"),
							new CsvColumn("level", "${level}"),
							new CsvColumn("message;text", "${message}"),
						},
					QuoteChar = "'",
					Delimiter = delim.Key,
					CustomColumnDelimiter = "zzz",
				};

				var ev = new LogEventInfo();
				ev.TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56);
				ev.Level = LogLevel.Info;
				ev.Message = "hello, world";

				string sep = delim.Value;
				csvLayout.DeepInitialize(CommonCfg);

				Assert.AreEqual("'2010-01-01 12:34:56.0000'" + sep + "'Info'" + sep + "'hello, world'", csvLayout.Render(ev));
				Assert.AreEqual("'date'" + sep + "'level'" + sep + "'message;text'", csvLayout.Header.Render(ev));
			}
		}

		[Test]
		public void CsvLayoutRenderingAutoQuoting()
		{
			var csvLayout = new CsvLayout()
			{
				Quoting = CsvQuotingMode.Auto,
				Columns =
					{
						new CsvColumn("date", "${longdate}"),
						new CsvColumn("level", "${level}"),
						new CsvColumn("message;text", "${message}"),
					},
				QuoteChar = "'",
				Delimiter = CsvColumnDelimiterMode.Semicolon,
			};

			csvLayout.DeepInitialize(CommonCfg);

			// no quoting
			Assert.AreEqual(
				"2010-01-01 12:34:56.0000;Info;hello, world",
				csvLayout.Render(new LogEventInfo
				{
					TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
					Level = LogLevel.Info,
					Message = "hello, world"
				}));

			// multi-line string - requires quoting
			Assert.AreEqual(
				"2010-01-01 12:34:56.0000;Info;'hello\rworld'",
				csvLayout.Render(new LogEventInfo
				{
					TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
					Level = LogLevel.Info,
					Message = "hello\rworld"
				}));

			// multi-line string - requires quoting
			Assert.AreEqual(
				"2010-01-01 12:34:56.0000;Info;'hello\nworld'",
				csvLayout.Render(new LogEventInfo
				{
					TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
					Level = LogLevel.Info,
					Message = "hello\nworld"
				}));

			// quote character used in string, will be quoted and doubled
			Assert.AreEqual(
				"2010-01-01 12:34:56.0000;Info;'hello''world'",
				csvLayout.Render(new LogEventInfo
				{
					TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
					Level = LogLevel.Info,
					Message = "hello'world"
				}));

			Assert.AreEqual("date;level;'message;text'", csvLayout.Header.Render(LogEventInfo.CreateNullEvent()));
		}

		[Test]
		public void CsvLayoutCachingTest()
		{
			var csvLayout = new CsvLayout()
			{
				Quoting = CsvQuotingMode.Auto,
				Columns =
					{
						new CsvColumn("date", "${longdate}"),
						new CsvColumn("level", "${level}"),
						new CsvColumn("message", "${message}"),
					},
				QuoteChar = "'",
				Delimiter = CsvColumnDelimiterMode.Semicolon,
			};

			var e1 = new LogEventInfo
			{
				TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
				Level = LogLevel.Info,
				Message = "hello, world"
			};

			var e2 = new LogEventInfo
			{
				TimeStamp = new DateTime(2010, 01, 01, 12, 34, 57),
				Level = LogLevel.Info,
				Message = "hello, world"
			};

			csvLayout.DeepInitialize(CommonCfg);

			var r11 = csvLayout.Render(e1);
			var r12 = csvLayout.Render(e1);
			var r21 = csvLayout.Render(e2);
			var r22 = csvLayout.Render(e2);

			var h11 = csvLayout.Header.Render(e1);
			var h12 = csvLayout.Header.Render(e1);
			var h21 = csvLayout.Header.Render(e2);
			var h22 = csvLayout.Header.Render(e2);

			Assert.AreSame(r11, r12);
			Assert.AreSame(r21, r22);

			Assert.AreNotSame(r11, r21);
			Assert.AreNotSame(r12, r22);

			Assert.AreSame(h11, h12);
			Assert.AreSame(h21, h22);

			Assert.AreNotSame(h11, h21);
			Assert.AreNotSame(h12, h22);
		}
	}
}
