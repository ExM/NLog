
using System.IO;
using System.Text;
using NUnit.Framework;

#if !NUNIT
	using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
	using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
	using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
	using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
using NLog.Layouts;
using NLog.Config;
#if SILVERLIGHT
using System.Xml.Linq;
#else
using System.Xml;
#endif

namespace NLog.UnitTests
{
	using System;
	using NLog.Internal;
	using NLog.Common;

	public abstract class NLogTestBase
	{
		public void AssertDebugCounter(string targetName, int val)
		{
			var debugTarget = (NLog.Targets.DebugTarget)LogManager.Configuration.FindTargetByName(targetName);

			Assert.IsNotNull(debugTarget, "Debug target '" + targetName + "' not found");
			Assert.AreEqual(val, debugTarget.Counter, "Unexpected counter value on '" + targetName + "'");
		}

		public void AssertDebugLastMessage(string targetName, string msg)
		{
			NLog.Targets.DebugTarget debugTarget = (NLog.Targets.DebugTarget)LogManager.Configuration.FindTargetByName(targetName);

			// Console.WriteLine("lastmsg: {0}", debugTarget.LastMessage);

			Assert.IsNotNull(debugTarget, "Debug target '" + targetName + "' not found");
			Assert.AreEqual(msg, debugTarget.LastMessage, "Unexpected last message value on '" + targetName + "'");
		}

		public string GetDebugLastMessage(string targetName)
		{
			var debugTarget = (NLog.Targets.DebugTarget)LogManager.Configuration.FindTargetByName(targetName);
			return debugTarget.LastMessage;
		}

		public void AssertFileContents(string fileName, string contents, Encoding encoding)
		{
			FileInfo fi = new FileInfo(fileName);
			if (!fi.Exists)
				Assert.Fail("File '" + fileName + "' doesn't exist.");

			byte[] encodedBuf = encoding.GetBytes(contents);
			Assert.AreEqual((long)encodedBuf.Length, fi.Length, "File length is incorrect.");
			byte[] buf = new byte[(int)fi.Length];
			using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				fs.Read(buf, 0, buf.Length);
			}

			for (int i = 0; i < buf.Length; ++i)
			{
				Assert.AreEqual(encodedBuf[i], buf[i], "File contents are different at position: #" + i);
			}
		}

		public string StringRepeat(int times, string s)
		{
			StringBuilder sb = new StringBuilder(s.Length * times);
			for (int i = 0; i < times; ++i)
				sb.Append(s);
			return sb.ToString();
		}

		protected void AssertLayoutRendererOutput(Layout l, string expected)
		{
			l.Initialize(null);
			string actual = l.Render(LogEventInfo.Create(LogLevel.Info, "loggername", "message"));
			l.Close();
			Assert.AreEqual(expected, actual);
		}

		protected XmlLoggingConfiguration CreateConfigurationFromString(string configXml)
		{
#if SILVERLIGHT
			XElement element = XElement.Parse(configXml);
			return new XmlLoggingConfiguration(element.CreateReader(), null);
#else
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(configXml);

#if NET_CF
			Console.WriteLine(CompactFrameworkHelper.GetExeBaseDir());
			return new XmlLoggingConfiguration(doc.DocumentElement, ".");
#else
			return new XmlLoggingConfiguration(doc.DocumentElement, Environment.CurrentDirectory);
#endif

#endif
		}

		protected string RunAndCaptureInternalLog(SyncAction action, LogLevel internalLogLevel)
		{
			var stringWriter = new StringWriter();
			var oldWriter = InternalLogger.LogWriter;
			var oldLevel = InternalLogger.LogLevel;
			var oldIncludeTimestamp = InternalLogger.IncludeTimestamp;
			try
			{
				InternalLogger.LogWriter = stringWriter;
				InternalLogger.LogLevel = LogLevel.Trace;
				InternalLogger.IncludeTimestamp = false;
				action();

				return stringWriter.ToString();
			}
			finally
			{
				InternalLogger.LogWriter = oldWriter;
				InternalLogger.LogLevel = oldLevel;
				InternalLogger.IncludeTimestamp = oldIncludeTimestamp;
			}
		}

		public delegate void SyncAction();

		public class InternalLoggerScope : IDisposable
		{
			private readonly string logFile;
			private readonly LogLevel logLevel;
			private readonly bool logToConsole;
			private readonly bool includeTimestamp;
#if !NET_CF
			private readonly bool logToConsoleError;
#endif
			private readonly LogLevel globalThreshold;
			private readonly bool throwExceptions;

			public InternalLoggerScope()
			{
				this.logFile = InternalLogger.LogFile;
				this.logLevel = InternalLogger.LogLevel;
				this.logToConsole = InternalLogger.LogToConsole;
				this.includeTimestamp = InternalLogger.IncludeTimestamp;
#if !NET_CF
				this.logToConsoleError = InternalLogger.LogToConsoleError;
#endif
				this.globalThreshold = LogManager.GlobalThreshold;
				this.throwExceptions = LogManager.ThrowExceptions;
			}

			public void Dispose()
			{
				InternalLogger.LogFile = this.logFile;
				InternalLogger.LogLevel = this.logLevel;
				InternalLogger.LogToConsole = this.logToConsole;
				InternalLogger.IncludeTimestamp = this.includeTimestamp;
#if !NET_CF
				InternalLogger.LogToConsoleError = this.logToConsoleError;
#endif
				LogManager.GlobalThreshold = this.globalThreshold;
				LogManager.ThrowExceptions = this.throwExceptions;
			}
		}
	}
}
