using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using NUnit.Framework;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using NLog.Common;

namespace NLog.UnitTests.Targets
{
	[TestFixture]
	public class ConcurrentFileTargetTests : NLogTestBase
	{
		private Logger logger = LogManager.GetLogger("NLog.UnitTests.Targets.ConcurrentFileTargetTests");

		private void ConfigureSharedFile(string mode)
		{
			FileTarget ft = new FileTarget();
			ft.FileName = "${basedir}/file.txt";
			ft.Layout = "${threadname} ${message}";
			ft.KeepFileOpen = true;
			ft.OpenFileCacheTimeout = 10;
			ft.OpenFileCacheSize = 1;
			ft.LineEnding = LineEndingMode.LF;

			switch (mode)
			{
				case "async":
					SimpleConfigurator.ConfigureForTargetLogging(new AsyncTargetWrapper(ft, 100, AsyncTargetWrapperOverflowAction.Grow), LogLevel.Debug);
					break;

				case "buffered":
					SimpleConfigurator.ConfigureForTargetLogging(new BufferingTargetWrapper(ft, 100), LogLevel.Debug);
					break;

				case "buffered_timed_flush":
					SimpleConfigurator.ConfigureForTargetLogging(new BufferingTargetWrapper(ft, 100, 10), LogLevel.Debug);
					break;

				default:
					SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);
					break;
			}
		}

		public void Process(string threadName, string numLogsString, string mode)
		{
			if (threadName != null)
			{
				Thread.CurrentThread.Name = threadName;
			}

			ConfigureSharedFile(mode);
			InternalLogger.LogLevel = LogLevel.Trace;
			InternalLogger.LogToConsole = true;
			int numLogs = Convert.ToInt32(numLogsString);
			for (int i = 0; i < numLogs; ++i)
			{
				logger.Debug("{0}", i);
			}
			
			LogManager.Configuration = null;
		}

		private void DoConcurrentTest(int numProcesses, int numLogs, string mode)
		{
			string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file.txt");

			if (File.Exists(logFile))
				File.Delete(logFile);

			Process[] processes = new Process[numProcesses];

			for (int i = 0; i < numProcesses; ++i)
			{
				processes[i] = ProcessRunner.SpawnMethod(this.GetType(), "Process", i.ToString(), numLogs.ToString(), mode);
			}
			
			for (int i = 0; i < numProcesses; ++i)
			{
				processes[i].WaitForExit();
				string output = processes[i].StandardOutput.ReadToEnd();
				Assert.AreEqual(0, processes[i].ExitCode, "Runner returned with an error. Standard output: " + output);
				processes[i].Dispose();
				processes[i] = null;
			}

			int[] maxNumber = new int[numProcesses];

			Console.WriteLine("Verifying output file {0}", logFile);
			using (StreamReader sr = File.OpenText(logFile))
			{
				string line;

				while ((line = sr.ReadLine()) != null)
				{
					string[] tokens = line.Split(' ');
					Assert.AreEqual(2, tokens.Length, "invalid output line: '" + line + "' expected two numbers.");
					try
					{
						int thread = Convert.ToInt32(tokens[0]);
						int number = Convert.ToInt32(tokens[1]);

						Assert.AreEqual(maxNumber[thread], number);
						maxNumber[thread]++;
					}
					catch (Exception ex)
					{
						throw new InvalidOperationException("Error when parsing line '" + line + "'", ex);
					}
				}
			}
		}

		private void DoConcurrentTest(string mode)
		{
			DoConcurrentTest(2, 10000, mode);
			DoConcurrentTest(5, 4000, mode);
			DoConcurrentTest(10, 2000, mode);
		}

		[Test]
		public void SimpleConcurrentTest()
		{
			DoConcurrentTest("none");
		}

		[Test]
		public void AsyncConcurrentTest()
		{
			DoConcurrentTest(2, 100, "async");
		}

		[Test]
		public void BufferedConcurrentTest()
		{
			DoConcurrentTest(2, 100, "buffered");
		}

		[Test]
		public void BufferedTimedFlushConcurrentTest()
		{
			DoConcurrentTest(2, 100, "buffered_timed_flush");
		}
	}
}
