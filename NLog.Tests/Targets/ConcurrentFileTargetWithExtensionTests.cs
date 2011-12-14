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
using System.Xml;
using Microsoft.CSharp;
using System.Collections.Generic;
using System.CodeDom.Compiler;

namespace NLog.UnitTests.Targets
{
	[TestFixture]
	public class ConcurrentFileTargetWithExtensionTests : NLogTestBase
	{
		[TestFixtureSetUp]
		public void CheckDll()
		{
			if (Platform.CurrentOS == PlatformID.Win32NT)
			{
				if (!File.Exists("NLog.WinTraits.dll"))
					Assert.Ignore("file NLog.WinTraits.dll not found");
			}
			else if (Platform.CurrentOS == PlatformID.Unix)
			{
				if (!File.Exists("NLog.UnixTraits.dll"))
					Assert.Ignore("file NLog.UnixTraits.dll not found");
			}
			else
				Assert.Ignore("unexpected OS: {0}", Platform.CurrentOS);


			if (File.Exists("Runner.exe"))
				File.Delete("Runner.exe");
			CompileRunner();
		}

		public void CompileRunner()
		{
			string sourceCode = @"
using System;
using System.Reflection;
using System.Threading;
using NLog;
using NLog.Common;
using NLog.Config;

class C1
{
	private static Logger logger = LogManager.GetCurrentClassLogger();

	static int Main(string[] args)
	{
		try
		{
			Thread.CurrentThread.Name = args[0];
			int numLogs = Convert.ToInt32(args[1]);
			
			for (int i = 0; i < numLogs; ++i)
				logger.Debug(""{0}"", i);
			
			LogManager.Flush();
			return 0;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
			return 1;
		}
	}
}";
			CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
			var options = new CompilerParameters();
			options.OutputAssembly = "Runner.exe";
			options.GenerateExecutable = true;
			options.ReferencedAssemblies.Add(typeof(Logger).Assembly.Location);
			options.ReferencedAssemblies.Add("System.dll");
			options.ReferencedAssemblies.Add("System.Core.dll");
			options.IncludeDebugInformation = false;
			var results = provider.CompileAssemblyFromSource(options, sourceCode);

			foreach (var err in results.Errors)
				Console.WriteLine(err.ToString());

			Assert.IsFalse(results.Errors.HasWarnings);
			Assert.IsFalse(results.Errors.HasErrors);
		}

		public static Process Run(int procNum, int numLogs, LineCollection coll)
		{
			Process proc = new Process();
			proc.StartInfo.Arguments = string.Format("{0} {1}", procNum, numLogs);
			proc.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Runner.exe");
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
			proc.StartInfo.RedirectStandardInput = false;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			proc.StartInfo.CreateNoWindow = true;

			proc.Start();

			proc.BeginOutputReadLine();
			proc.OutputDataReceived += (s, e) => coll.AppendLine("Process {0}: `{1}'", procNum, e.Data);
			
			return proc;
		}

		private void DoConcurrentTest(int numProcesses, int numLogs)
		{
			string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file.txt");

			if(File.Exists(logFile))
				File.Delete(logFile);

			LineCollection coll = new LineCollection();
			Process[] processes = new Process[numProcesses];

			for(int i = 0; i < numProcesses; ++i)
				processes[i] = Run(i, numLogs, coll);
			
			bool errorFound = false;
			
			for(int i = 0; i < numProcesses; ++i)
			{
				processes[i].WaitForExit();
				if(processes[i].ExitCode != 0)
					errorFound = true;
				processes[i].Dispose();
				processes[i] = null;
			}
			
			if(errorFound)
			{
				Console.WriteLine(coll.BuildLines());
				Assert.Fail("Runner returned with an error.");
			}

			int[] maxNumber = new int[numProcesses];

			using(StreamReader sr = File.OpenText(logFile))
			{
				string line;

				while((line = sr.ReadLine()) != null)
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
					catch(Exception ex)
					{
						throw new InvalidOperationException("Error when parsing line '" + line + "'", ex);
					}
				}
			}
		}

		string _cfgFrmContent =
@"<configuration>
	<configSections>
		<section name='nlog' type='NLog.Config.ConfigSectionHandler, NLog'/>
	</configSections>
	<nlog throwExceptions='true' internalLogLevel='Trace' internalLogToConsole='true' >
		<extensions>
			<add platform='Win32NT' assemblyFile='NLog.WinTraits.dll' />
			<add platform='Unix' assemblyFile='NLog.UnixTraits.dll' />
		</extensions>

		<targets>
{0}
		</targets>

		<rules>
			<logger name='*' writeTo='t' LogLevel='Debug'/>
		</rules>
	</nlog>
</configuration>";

		private void ConfigEnvironment(string targets, Action action)
		{
			File.WriteAllText("Runner.exe.config", string.Format(_cfgFrmContent, targets));
			action();
			File.Delete("Runner.exe.config");
		}


		[Test]
		public void SimpleConcurrentTest_2_10000()
		{
			SimpleConcurrentTest(2, 10000);
		}

		[Test]
		public void SimpleConcurrentTest_5_4000()
		{
			SimpleConcurrentTest(5, 4000);
		}

		[Test]
		public void SimpleConcurrentTest_10_2000()
		{
			SimpleConcurrentTest(10, 2000);
		}

		public void SimpleConcurrentTest(int numProcesses, int numLogs)
		{
			string target = @"
			<target name='t' type='File'
				fileName='${basedir}/file.txt'
				layout='${threadname} ${message}'
				keepFileOpen='true'
				openFileCacheTimeout='10'
				openFileCacheSize='1'
				lineEnding='LF'
				concurrentWrites='true'/>";

			ConfigEnvironment(target, () => DoConcurrentTest(numProcesses, numLogs));
		}

		[Test]
		public void AsyncConcurrentTest()
		{
			string target = @"
			<target name='t' type='AsyncWrapper' queueLimit='10' overflowAction='Grow'>
				<target type='File'
					fileName='${basedir}/file.txt'
					layout='${threadname} ${message}'
					keepFileOpen='true'
					openFileCacheTimeout='10'
					openFileCacheSize='1'
					lineEnding='LF'
					concurrentWrites='true'/>
			</target>";

			ConfigEnvironment(target, () => DoConcurrentTest(2, 1000));
		}
		
		[Test]
		public void BufferedConcurrentTest()
		{
			string target = @"
			<target name='t' type='BufferingWrapper' bufferSize='10'>
				<target type='File'
					fileName='${basedir}/file.txt'
					layout='${threadname} ${message}'
					keepFileOpen='true'
					openFileCacheTimeout='10'
					openFileCacheSize='1'
					lineEnding='LF'
					concurrentWrites='true'/>
			</target>";

			ConfigEnvironment(target, () => DoConcurrentTest(2, 100));
		}

		[Test]
		public void BufferedTimedFlushConcurrentTest()
		{
			string target = @"
			<target name='t' type='BufferingWrapper' bufferSize='100' flushTimeout='10'>
				<target type='File'
					fileName='${basedir}/file.txt'
					layout='${threadname} ${message}'
					keepFileOpen='true'
					openFileCacheTimeout='10'
					openFileCacheSize='1'
					lineEnding='LF'
					concurrentWrites='true'/>
			</target>";

			ConfigEnvironment(target, () => DoConcurrentTest(2, 1000));
		}
	}
}
