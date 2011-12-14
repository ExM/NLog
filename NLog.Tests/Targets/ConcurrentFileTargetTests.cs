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
	public class ConcurrentFileTargetTests : NLogTestBase
	{
		[TestFixtureSetUp]
		public void CheckDll()
		{
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

		private void DoConcurrentTest(int numProcesses, int numLogs, string mode)
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

		string _defaultCfgContent =
@"<configuration>
	<configSections>
		<section name='nlog' type='NLog.Config.ConfigSectionHandler, NLog'/>
	</configSections>
	<nlog throwExceptions='true' internalLogLevel='Trace' internalLogToConsole='true' >
		<extensions>
			<!--add platform='Win32NT' assemblyFile='NLog.WinTraits.dll' /-->
			<!--add platform='Unix' assemblyFile='NLog.UnixTraits.dll' /-->
		</extensions>

		<targets>
			<target name='t' type='File'
				fileName='${basedir}/file.txt'
				layout='${threadname} ${message}'
				keepFileOpen='true'
				openFileCacheTimeout='10'
				openFileCacheSize='1'
				lineEnding='LF'
				concurrentWrites='true'/>
		</targets>

		<rules>
			<logger name='*' writeTo='t' LogLevel='Debug'/>
		</rules>
	</nlog>
</configuration>";

		[Test]
		public void SimpleConcurrentTest()
		{
			if(Platform.CurrentOS == PlatformID.Unix)
				Assert.Ignore("lines may be blanks for mono");
			
			File.WriteAllText("Runner.exe.config", _defaultCfgContent);

			DoConcurrentTest(2, 1000, "none");
			//More processes requires an increase in the number attempts to write.

			File.Delete("Runner.exe.config");
		}
	}
}
