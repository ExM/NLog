using System.IO;
using System.Text;
using NUnit.Framework;
using NLog.Layouts;
using NLog.Config;
using System.Xml;
using System;
using NLog.Internal;
using NLog.Common;

namespace NLog.UnitTests
{
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
			l.Initialize(new LoggingConfiguration());
			string actual = l.Render(LogEventInfo.Create(LogLevel.Info, "loggername", "message"));
			l.Close();
			Assert.AreEqual(expected, actual);
		}

		protected XmlLoggingConfiguration CreateConfigurationFromString(string configXml)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(configXml);
			return new XmlLoggingConfiguration(doc.DocumentElement, Environment.CurrentDirectory);
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
			private readonly bool logToConsoleError;
			private readonly LogLevel globalThreshold;
			private readonly bool throwExceptions;

			public InternalLoggerScope()
			{
				this.logFile = InternalLogger.LogFile;
				this.logLevel = InternalLogger.LogLevel;
				this.logToConsole = InternalLogger.LogToConsole;
				this.includeTimestamp = InternalLogger.IncludeTimestamp;
				this.logToConsoleError = InternalLogger.LogToConsoleError;
				this.globalThreshold = LogManager.GlobalThreshold;
				this.throwExceptions = LogManager.ThrowExceptions;
			}

			public void Dispose()
			{
				InternalLogger.LogFile = this.logFile;
				InternalLogger.LogLevel = this.logLevel;
				InternalLogger.LogToConsole = this.logToConsole;
				InternalLogger.IncludeTimestamp = this.includeTimestamp;
				InternalLogger.LogToConsoleError = this.logToConsoleError;
				LogManager.GlobalThreshold = this.globalThreshold;
				LogManager.ThrowExceptions = this.throwExceptions;
			}
		}
	}
}
