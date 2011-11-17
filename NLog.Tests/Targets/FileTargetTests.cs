using System.Diagnostics;
using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System.Threading;
using NLog.Internal;
using System.Collections.Generic;

namespace NLog.UnitTests.Targets
{
	[TestFixture]
	public class FileTargetTests : NLogTestBase
	{
		private Logger logger = LogManager.GetLogger("NLog.UnitTests.Targets.FileTargetTests");

		[Test]
		public void SimpleFileTest1()
		{
			string tempFile = Path.GetTempFileName();
			try
			{
				FileTarget ft = new FileTarget();
				ft.FileName = SimpleLayout.Escape(tempFile);
				ft.LineEnding = LineEndingMode.LF;
				ft.Layout = "${level} ${message}";
				ft.OpenFileCacheTimeout = 0;

				SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

				logger.Debug("aaa");
				logger.Info("bbb");
				logger.Warn("ccc");
				LogManager.Configuration = null;
				AssertFileContents(tempFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
			}
			finally
			{
				if (File.Exists(tempFile))
					File.Delete(tempFile);
			}
		}

		[Test]
		public void DeleteFileOnStartTest()
		{
			string tempFile = Path.GetTempFileName();
			try
			{
				FileTarget ft = new FileTarget();
				ft.FileName = SimpleLayout.Escape(tempFile);
				ft.LineEnding = LineEndingMode.LF;
				ft.Layout = "${level} ${message}";

				SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

				logger.Debug("aaa");
				logger.Info("bbb");
				logger.Warn("ccc");

				LogManager.Configuration = null;

				AssertFileContents(tempFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);

				// configure again, without
				// DeleteOldFileOnStartup

				ft = new FileTarget();
				ft.FileName = SimpleLayout.Escape(tempFile);
				ft.LineEnding = LineEndingMode.LF;
				ft.Layout = "${level} ${message}";

				SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

				logger.Debug("aaa");
				logger.Info("bbb");
				logger.Warn("ccc");

				LogManager.Configuration = null;
				AssertFileContents(tempFile, "Debug aaa\nInfo bbb\nWarn ccc\nDebug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);

				// configure again, this time with
				// DeleteOldFileOnStartup

				ft = new FileTarget();
				ft.FileName = SimpleLayout.Escape(tempFile);
				ft.LineEnding = LineEndingMode.LF;
				ft.Layout = "${level} ${message}";
				ft.DeleteOldFileOnStartup = true;

				SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);
				logger.Debug("aaa");
				logger.Info("bbb");
				logger.Warn("ccc");

				LogManager.Configuration = null;
				AssertFileContents(tempFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
			}
			finally
			{
				LogManager.Configuration = null;
				if (File.Exists(tempFile))
					File.Delete(tempFile);
			}
		}

		[Test]
		public void CreateDirsTest()
		{
			// create the file in a not-existent
			// directory which forces creation
			string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			string tempFile = Path.Combine(tempPath, "file.txt");
			try
			{
				FileTarget ft = new FileTarget();
				ft.FileName = tempFile;
				ft.LineEnding = LineEndingMode.LF;
				ft.Layout = "${level} ${message}";

				SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

				logger.Debug("aaa");
				logger.Info("bbb");
				logger.Warn("ccc");
				LogManager.Configuration = null;
				AssertFileContents(tempFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
			}
			finally
			{
				LogManager.Configuration = null;
				if (File.Exists(tempFile))
					File.Delete(tempFile);
				if (Directory.Exists(tempPath))
					Directory.Delete(tempPath, true);
			}
		}

		[Test]
		public void SequentialArchiveTest1()
		{
			// create the file in a not-existent
			// directory which forces creation
			string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			string tempFile = Path.Combine(tempPath, "file.txt");
			try
			{
				FileTarget ft = new FileTarget();
				ft.FileName = tempFile;
				ft.ArchiveFileName = Path.Combine(tempPath, "archive/{####}.txt");
				ft.ArchiveAboveSize = 1000;
				ft.LineEnding = LineEndingMode.LF;
				ft.Layout = "${message}";
				ft.MaxArchiveFiles = 3;
				ft.ArchiveNumbering = ArchiveNumberingMode.Sequence;

				SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

				// we emit 5 * 250 *(3 x aaa + \n) bytes
				// so that we should get a full file + 3 archives
				for (int i = 0; i < 250; ++i)
				{
					logger.Debug("aaa");
				}
				for (int i = 0; i < 250; ++i)
				{
					logger.Debug("bbb");
				}
				for (int i = 0; i < 250; ++i)
				{
					logger.Debug("ccc");
				}
				for (int i = 0; i < 250; ++i)
				{
					logger.Debug("ddd");
				}
				for (int i = 0; i < 250; ++i)
				{
					logger.Debug("eee");
				}

				LogManager.Configuration = null;

				AssertFileContents(tempFile,
					StringRepeat(250, "eee\n"),
					Encoding.UTF8);

				AssertFileContents(
					Path.Combine(tempPath, "archive/0001.txt"),
					StringRepeat(250, "bbb\n"),
					Encoding.UTF8);

				AssertFileContents(
					Path.Combine(tempPath, "archive/0002.txt"),
					StringRepeat(250, "ccc\n"),
					Encoding.UTF8);

				AssertFileContents(
					Path.Combine(tempPath, "archive/0003.txt"),
					StringRepeat(250, "ddd\n"),
					Encoding.UTF8);

				Assert.IsTrue(!File.Exists(Path.Combine(tempPath, "archive/0000.txt")));
				Assert.IsTrue(!File.Exists(Path.Combine(tempPath, "archive/0004.txt")));
			}
			finally
			{
				LogManager.Configuration = null;
				if (File.Exists(tempFile))
					File.Delete(tempFile);
				if (Directory.Exists(tempPath))
					Directory.Delete(tempPath, true);
			}
		}

		[Test]
		public void RollingArchiveTest1()
		{
			// create the file in a not-existent
			// directory which forces creation
			string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			string tempFile = Path.Combine(tempPath, "file.txt");
			try
			{
				FileTarget ft = new FileTarget();
				ft.FileName = tempFile;
				ft.ArchiveFileName = Path.Combine(tempPath, "archive/{####}.txt");
				ft.ArchiveAboveSize = 1000;
				ft.LineEnding = LineEndingMode.LF;
				ft.ArchiveNumbering = ArchiveNumberingMode.Rolling;
				ft.Layout = "${message}";
				ft.MaxArchiveFiles = 3;

				SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

				// we emit 5 * 250 * (3 x aaa + \n) bytes
				// so that we should get a full file + 3 archives
				for (int i = 0; i < 250; ++i)
				{
					logger.Debug("aaa");
				}
				for (int i = 0; i < 250; ++i)
				{
					logger.Debug("bbb");
				}
				for (int i = 0; i < 250; ++i)
				{
					logger.Debug("ccc");
				}
				for (int i = 0; i < 250; ++i)
				{
					logger.Debug("ddd");
				}
				for (int i = 0; i < 250; ++i)
				{
					logger.Debug("eee");
				}

				LogManager.Configuration = null;

				AssertFileContents(tempFile,
					StringRepeat(250, "eee\n"),
					Encoding.UTF8);

				AssertFileContents(
					Path.Combine(tempPath, "archive/0000.txt"),
					StringRepeat(250, "ddd\n"),
					Encoding.UTF8);

				AssertFileContents(
					Path.Combine(tempPath, "archive/0001.txt"),
					StringRepeat(250, "ccc\n"),
					Encoding.UTF8);

				AssertFileContents(
					Path.Combine(tempPath, "archive/0002.txt"),
					StringRepeat(250, "bbb\n"),
					Encoding.UTF8);

				Assert.IsTrue(!File.Exists(Path.Combine(tempPath, "archive/0003.txt")));
			}
			finally
			{
				LogManager.Configuration = null;
				if (File.Exists(tempFile))
					File.Delete(tempFile);
				if (Directory.Exists(tempPath))
					Directory.Delete(tempPath, true);
			}
		}

		[Test]
		public void MultiFileWrite()
		{
			// create the file in a not-existent
			// directory which forces creation
			string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			try
			{
				FileTarget ft = new FileTarget();
				ft.FileName = Path.Combine(tempPath, "${level}.txt");
				ft.LineEnding = LineEndingMode.LF;
				ft.Layout = "${message}";

				SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

				for (int i = 0; i < 250; ++i)
				{
					logger.Trace("@@@");
					logger.Debug("aaa");
					logger.Info("bbb");
					logger.Warn("ccc");
					logger.Error("ddd");
					logger.Fatal("eee");
				}

				LogManager.Configuration = null;

				Assert.IsFalse(File.Exists(Path.Combine(tempPath, "Trace.txt")));

				AssertFileContents(Path.Combine(tempPath, "Debug.txt"),
					StringRepeat(250, "aaa\n"), Encoding.UTF8);

				AssertFileContents(Path.Combine(tempPath, "Info.txt"),
					StringRepeat(250, "bbb\n"), Encoding.UTF8);

				AssertFileContents(Path.Combine(tempPath, "Warn.txt"),
					StringRepeat(250, "ccc\n"), Encoding.UTF8);

				AssertFileContents(Path.Combine(tempPath, "Error.txt"),
					StringRepeat(250, "ddd\n"), Encoding.UTF8);

				AssertFileContents(Path.Combine(tempPath, "Fatal.txt"),
					StringRepeat(250, "eee\n"), Encoding.UTF8);
			}
			finally
			{
				//if (File.Exists(tempFile))
				//	File.Delete(tempFile);
				LogManager.Configuration = null;
				if (Directory.Exists(tempPath))
					Directory.Delete(tempPath, true);
			}
		}

		[Test]
		public void BufferedMultiFileWrite()
		{
			// create the file in a not-existent
			// directory which forces creation
			string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			try
			{
				FileTarget ft = new FileTarget();
				ft.FileName = Path.Combine(tempPath, "${level}.txt");
				ft.LineEnding = LineEndingMode.LF;
				ft.Layout = "${message}";

				SimpleConfigurator.ConfigureForTargetLogging(new BufferingTargetWrapper(ft, 10), LogLevel.Debug);

				for (int i = 0; i < 250; ++i)
				{
					logger.Trace("@@@");
					logger.Debug("aaa");
					logger.Info("bbb");
					logger.Warn("ccc");
					logger.Error("ddd");
					logger.Fatal("eee");
				}

				LogManager.Configuration = null;

				Assert.IsFalse(File.Exists(Path.Combine(tempPath, "Trace.txt")));

				AssertFileContents(Path.Combine(tempPath, "Debug.txt"),
					StringRepeat(250, "aaa\n"), Encoding.UTF8);

				AssertFileContents(Path.Combine(tempPath, "Info.txt"),
					StringRepeat(250, "bbb\n"), Encoding.UTF8);

				AssertFileContents(Path.Combine(tempPath, "Warn.txt"),
					StringRepeat(250, "ccc\n"), Encoding.UTF8);

				AssertFileContents(Path.Combine(tempPath, "Error.txt"),
					StringRepeat(250, "ddd\n"), Encoding.UTF8);

				AssertFileContents(Path.Combine(tempPath, "Fatal.txt"),
					StringRepeat(250, "eee\n"), Encoding.UTF8);
			}
			finally
			{
				//if (File.Exists(tempFile))
				//	File.Delete(tempFile);
				LogManager.Configuration = null;
				if (Directory.Exists(tempPath))
					Directory.Delete(tempPath, true);
			}
		}

		[Test]
		public void AsyncMultiFileWrite()
		{
			//InternalLogger.LogToConsole = true;
			//InternalLogger.LogLevel = LogLevel.Trace;

			string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			try
			{
				FileTarget ft = new FileTarget();
				ft.FileName = Path.Combine(tempPath, "${level}.txt");
				ft.LineEnding = LineEndingMode.LF;
				ft.Layout = "${message} ${threadid}";

				// this also checks that thread-volatile layouts
				// such as ${threadid} are properly cached and not recalculated
				// in logging threads.

				string threadID = Thread.CurrentThread.ManagedThreadId.ToString();

				//InternalLogger.LogToConsole = true;
				//InternalLogger.LogLevel = LogLevel.Trace;
				SimpleConfigurator.ConfigureForTargetLogging(new AsyncTargetWrapper(ft, 1000, AsyncTargetWrapperOverflowAction.Grow), LogLevel.Debug);
				LogManager.ThrowExceptions = true;

				for (int i = 0; i < 250; ++i)
				{
					logger.Trace("@@@");
					logger.Debug("aaa");
					logger.Info("bbb");
					logger.Warn("ccc");
					logger.Error("ddd");
					logger.Fatal("eee");
				}
				LogManager.Flush();
				LogManager.Configuration = null;

				Assert.IsFalse(File.Exists(Path.Combine(tempPath, "Trace.txt")));

				AssertFileContents(Path.Combine(tempPath, "Debug.txt"),
					StringRepeat(250, "aaa " + threadID + "\n"), Encoding.UTF8);

				AssertFileContents(Path.Combine(tempPath, "Info.txt"),
					StringRepeat(250, "bbb " + threadID + "\n"), Encoding.UTF8);

				AssertFileContents(Path.Combine(tempPath, "Warn.txt"),
					StringRepeat(250, "ccc " + threadID + "\n"), Encoding.UTF8);

				AssertFileContents(Path.Combine(tempPath, "Error.txt"),
					StringRepeat(250, "ddd " + threadID + "\n"), Encoding.UTF8);

				AssertFileContents(Path.Combine(tempPath, "Fatal.txt"),
					StringRepeat(250, "eee " + threadID + "\n"), Encoding.UTF8);
			}
			finally
			{
				//if (File.Exists(tempFile))
				//	File.Delete(tempFile);
				LogManager.Configuration = null;
				if (Directory.Exists(tempPath))
					Directory.Delete(tempPath, true);
			}
		}


		[Test]
		public void BatchErrorHandlingTest()
		{
			var fileTarget = new FileTarget();
			fileTarget.FileName = "${logger}";
			fileTarget.Layout = "${message}";
			fileTarget.Initialize(CommonCfg);

			// make sure that when file names get sorted, the asynchronous continuations are sorted with them as well
			var exceptions = new List<Exception>();
			var events = new[]
			{
				new LogEventInfo(LogLevel.Info, "file99.txt", "msg1").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "a/", "msg1").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "a/", "msg2").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "a/", "msg3").WithContinuation(exceptions.Add),
			};

			fileTarget.WriteAsyncLogEvents(events);

			Assert.AreEqual(4, exceptions.Count);
			Assert.IsNull(exceptions[0]);
			Assert.IsNotNull(exceptions[1]);
			Assert.IsNotNull(exceptions[2]);
			Assert.IsNotNull(exceptions[3]);
		}
	}
}
