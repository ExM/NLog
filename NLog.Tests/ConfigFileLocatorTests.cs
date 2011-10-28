using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.CSharp;
using NUnit.Framework;

namespace NLog.UnitTests
{
	[TestFixture]
	public class ConfigFileLocatorTests
	{
		private string appConfigContents = @"
<configuration>
<configSections>
	<section name='nlog' type='NLog.Config.ConfigSectionHandler, NLog' requirePermission='false' />
</configSections>

<nlog>
  <targets>
	<target name='c' type='Console' layout='AC ${message}' />
  </targets>
  <rules>
	<logger name='*' minLevel='Info' writeTo='c' />
  </rules>
</nlog>
</configuration>
";

		private string appNLogContents = @"
<nlog>
  <targets>
	<target name='c' type='Console' layout='AN ${message}' />
  </targets>
  <rules>
	<logger name='*' minLevel='Info' writeTo='c' />
  </rules>
</nlog>
";

		private string nlogConfigContents = @"
<nlog>
  <targets>
	<target name='c' type='Console' layout='NLC ${message}' />
  </targets>
  <rules>
	<logger name='*' minLevel='Info' writeTo='c' />
  </rules>
</nlog>
";

		private string nlogDllNLogContents = @"
<nlog>
  <targets>
	<target name='c' type='Console' layout='NDN ${message}' />
  </targets>
  <rules>
	<logger name='*' minLevel='Info' writeTo='c' />
  </rules>
</nlog>
";

		private string appConfigOutput = "--BEGIN--|AC InfoMsg|AC WarnMsg|AC ErrorMsg|AC FatalMsg|--END--|";
		private string appNLogOutput = "--BEGIN--|AN InfoMsg|AN WarnMsg|AN ErrorMsg|AN FatalMsg|--END--|";
		private string nlogConfigOutput = "--BEGIN--|NLC InfoMsg|NLC WarnMsg|NLC ErrorMsg|NLC FatalMsg|--END--|";
		private string nlogDllNLogOutput = "--BEGIN--|NDN InfoMsg|NDN WarnMsg|NDN ErrorMsg|NDN FatalMsg|--END--|";
		private string missingConfigOutput = "--BEGIN--|--END--|";

		[Test]
		public void MissingConfigFileTest()
		{
			string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

			Directory.CreateDirectory(dir);
			try
			{
				string output = RunTestIn(dir);
				Assert.AreEqual(missingConfigOutput, output);
			}
			finally
			{
				Directory.Delete(dir, true);
			}
		}

		[Test]
		public void NLogDotConfigTest()
		{
			string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

			Directory.CreateDirectory(dir);
			try
			{
				File.WriteAllText(Path.Combine(dir, "NLog.config"), nlogConfigContents);
				string output = RunTestIn(dir);
				Assert.AreEqual(nlogConfigOutput, output);
			}
			finally
			{
				Directory.Delete(dir, true);
			}
		}

		[Test]
		public void NLogDotDllDotNLogTest()
		{
			string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

			Directory.CreateDirectory(dir);
			try
			{
				File.WriteAllText(Path.Combine(dir, "NLog.dll.nlog"), nlogDllNLogContents);
				string output = RunTestIn(dir);
				Assert.AreEqual(nlogDllNLogOutput, output);
			}
			finally
			{
				Directory.Delete(dir, true);
			}
		}

		[Test]
		public void NLogDotDllDotNLogInDirectoryWithSpaces()
		{
			string dir = Path.Combine(Path.GetTempPath(), "abc " + Guid.NewGuid().ToString("N") + " def");

			Directory.CreateDirectory(dir);
			try
			{
				File.WriteAllText(Path.Combine(dir, "NLog.dll.nlog"), nlogDllNLogContents);
				string output = RunTestIn(dir);
				Assert.AreEqual(nlogDllNLogOutput, output);
			}
			finally
			{
				Directory.Delete(dir, true);
			}
		}

		[Test]
		public void AppDotConfigTest()
		{
			string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

			Directory.CreateDirectory(dir);
			try
			{
				File.WriteAllText(Path.Combine(dir, "ConfigFileLocator.exe.config"), appConfigContents);
				string output = RunTestIn(dir);
				Assert.AreEqual(appConfigOutput, output);
			}
			finally
			{
				Directory.Delete(dir, true);
			}
		}

		[Test]
		public void AppDotNLogTest()
		{
			string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

			Directory.CreateDirectory(dir);
			try
			{
				File.WriteAllText(Path.Combine(dir, "ConfigFileLocator.exe.nlog"), appNLogContents);
				string output = RunTestIn(dir);
				Assert.AreEqual(appNLogOutput, output);
			}
			finally
			{
				Directory.Delete(dir, true);
			}
		}

		[Test]
		public void PrecedenceTest()
		{
			var precedence = new[]
								 {
									 new
										 {
											 File = "ConfigFileLocator.exe.config",
											 Contents = appConfigContents,
											 Output = appConfigOutput
										 },
									 new
										 {
											 File = "NLog.config",
											 Contents = nlogConfigContents,
											 Output = nlogConfigOutput
										 },
									 new
										 {
											 File = "ConfigFileLocator.exe.nlog",
											 Contents = appNLogContents,
											 Output = appNLogOutput
										 },
									 new
										 {
											 File = "NLog.dll.nlog",
											 Contents = nlogDllNLogContents,
											 Output = nlogDllNLogOutput
										 },
								 };
			string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

			Directory.CreateDirectory(dir);
			try
			{
				// deploy all files
				foreach (var p in precedence)
				{
					File.WriteAllText(Path.Combine(dir, p.File), p.Contents);
				}

				string output;

				// walk files in precedence order and delete config files
				foreach (var p in precedence)
				{
					output = RunTestIn(dir);
					Assert.AreEqual(p.Output, output);
					File.Delete(Path.Combine(dir, p.File));
				}

				output = RunTestIn(dir);
				Assert.AreEqual(missingConfigOutput, output);

			}
			finally
			{
				Directory.Delete(dir, true);
			}

		}

		private static string RunTestIn(string directory)
		{
		string sourceCode = @"
using System;
using System.Reflection;
using NLog;

class C1
{
	private static Logger logger = LogManager.GetCurrentClassLogger();

	static void Main(string[] args)
	{
		Console.WriteLine(""--BEGIN--"");
		logger.Trace(""TraceMsg"");
		logger.Debug(""DebugMsg"");
		logger.Info(""InfoMsg"");
		logger.Warn(""WarnMsg"");
		logger.Error(""ErrorMsg"");
		logger.Fatal(""FatalMsg"");
		Console.WriteLine(""--END--"");
	}
}";
			var provider = new CSharpCodeProvider();
			var options = new CompilerParameters();
			options.OutputAssembly = Path.Combine(directory, "ConfigFileLocator.exe");
			options.GenerateExecutable = true;
			options.ReferencedAssemblies.Add(typeof(Logger).Assembly.Location);
			options.IncludeDebugInformation = true;
			if (!File.Exists(options.OutputAssembly))
			{
				var results = provider.CompileAssemblyFromSource(options, sourceCode);
				Assert.IsFalse(results.Errors.HasWarnings);
				Assert.IsFalse(results.Errors.HasErrors);
				File.Copy(typeof (Logger).Assembly.Location, Path.Combine(directory, "NLog.dll"));
			}

			return RunAndRedirectOutput(options.OutputAssembly);
		}

		public static string RunAndRedirectOutput(string exeFile)
		{
			using (var proc = new Process())
			{
				proc.StartInfo.Arguments = "";
				proc.StartInfo.FileName = exeFile;

				proc.StartInfo.UseShellExecute = false;
				proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
				proc.StartInfo.RedirectStandardInput = false;
				proc.StartInfo.RedirectStandardOutput = true;
				proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				proc.StartInfo.CreateNoWindow = true;
				proc.Start();
				
				if(!proc.WaitForExit(10000))
				{
					proc.Kill();
					Assert.Fail("process hung");
				}
				
				return proc.StandardOutput.ReadToEnd().Replace("\r", "").Replace("\n", "|");
			}
		}
	}
}
