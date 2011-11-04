using System;
using System.Xml;
using System.Reflection;
using System.IO;
using NLog;
using NLog.Config;
using NUnit.Framework;

namespace NLog.UnitTests.LayoutRenderers
{
	using System.Threading;
	using NLog.Internal;
	using System.Diagnostics;

	[TestFixture]
	public class Log4JXmlTests : NLogTestBase
	{
		[Test]
		public void Log4JXmlTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(
@"
<nlog throwExceptions='true'>
  <extensions> 
	<add assembly='NLog.Network'/> 
  </extensions>
  <targets>
	<target name='debug' type='Debug' layout='${log4jxmlevent:includeCallSite=true:includeSourceInfo=true:includeMdc=true:includendc=true:ndcItemSeparator=\:\::includenlogdata=true}' />
  </targets>
  <rules>
	<logger name='*' minlevel='Debug' writeTo='debug' />
  </rules>
  </nlog>");

			MappedDiagnosticsContext.Clear();
			NestedDiagnosticsContext.Clear();

			MappedDiagnosticsContext.Set("foo1", "bar1");
			MappedDiagnosticsContext.Set("foo2", "bar2");

			NestedDiagnosticsContext.Push("baz1");
			NestedDiagnosticsContext.Push("baz2");
			NestedDiagnosticsContext.Push("baz3");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("some message");
			string result = GetDebugLastMessage("debug");
			string wrappedResult = "<log4j:dummyRoot xmlns:log4j='http://log4j' xmlns:nlog='http://nlog'>" + result + "</log4j:dummyRoot>";

			Assert.AreNotEqual("", result);
			// make sure the XML can be read back and verify some fields
			StringReader stringReader = new StringReader(wrappedResult);
			using (XmlReader reader = XmlReader.Create(stringReader))
			{
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element && reader.Prefix == "log4j")
					{
						switch (reader.LocalName)
						{
							case "dummyRoot":
								break;

							case "event":
								Assert.AreEqual("DEBUG", reader.GetAttribute("level"));
								Assert.AreEqual("A", reader.GetAttribute("logger"));

								var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
								long timestamp = Convert.ToInt64(reader.GetAttribute("timestamp"));
								var time = epochStart.AddMilliseconds(timestamp);
								var now = DateTime.UtcNow;
								Assert.IsTrue(now.Ticks - time.Ticks < TimeSpan.FromSeconds(3).Ticks);

								Assert.AreEqual(Thread.CurrentThread.ManagedThreadId.ToString(), reader.GetAttribute("thread"));
								break;

							case "message":
								reader.Read();
								Assert.AreEqual("some message", reader.Value);
								break;

							case "NDC":
								reader.Read();
								Assert.AreEqual("baz3::baz2::baz1", reader.Value);
								break;

							case "locationInfo":
								Assert.AreEqual(MethodBase.GetCurrentMethod().DeclaringType.FullName, reader.GetAttribute("class"));
								Assert.AreEqual(MethodBase.GetCurrentMethod().ToString(), reader.GetAttribute("method"));
								break;

							case "properties":
								break;

							case "data":
								string name = reader.GetAttribute("name");
								string value = reader.GetAttribute("value");

								switch (name)
								{
									case "log4japp":
										Assert.AreEqual(AppDomain.CurrentDomain.FriendlyName + "(" + Process.GetCurrentProcess().Id + ")", value);
										break;

									case "log4jmachinename":
										Assert.AreEqual(Environment.MachineName, value);
										break;

									case "foo1":
										Assert.AreEqual("bar1", value);
										break;

									case "foo2":
										Assert.AreEqual("bar2", value);
										break;

									default:
										Assert.Fail("Unknown <log4j:data>: " + name);
										break;
								}
								break;

							default:
								throw new NotSupportedException("Unknown element: " + reader.LocalName);
						}
						continue;
					}

					if (reader.NodeType == XmlNodeType.Element && reader.Prefix == "nlog")
					{
						switch (reader.LocalName)
						{
							case "eventSequenceNumber":
								break;

							case "locationInfo":
								Assert.AreEqual(this.GetType().Assembly.FullName, reader.GetAttribute("assembly"));
								break;

							default:
								throw new NotSupportedException("Unknown element: " + reader.LocalName);
						}
					}
				}
			}
		}
	}
}