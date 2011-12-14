using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.CSharp;
using NUnit.Framework;
using System.Collections.Generic;

namespace NLog.UnitTests
{
	public static class ProcessRunner
	{
		static ProcessRunner()
		{
			string sourceCode = @"
using System;
using System.Reflection;

class C1
{
	static int Main(string[] args)
	{
		try
		{
			string assemblyName = args[0];
			string className = args[1];
			string methodName = args[2];
			object[] arguments = new object[args.Length - 3];
			for (int i = 0; i < arguments.Length; ++i)
				arguments[i] = args[3 + i];

			Assembly assembly = Assembly.Load(assemblyName);
			Type type = assembly.GetType(className);
			if (type == null)
				throw new Exception(className + "" not found in "" + assemblyName);
			MethodInfo method = type.GetMethod(methodName);
			if (method == null)
				throw new Exception(methodName + "" not found in "" + type);
			object targetObject = null;
			if (!method.IsStatic)
				targetObject = Activator.CreateInstance(type);
			method.Invoke(targetObject, arguments);
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
			options.ReferencedAssemblies.Add("System.dll");
			options.ReferencedAssemblies.Add("System.Core.dll");
			options.IncludeDebugInformation = true;
			var results = provider.CompileAssemblyFromSource(options, sourceCode);
			Assert.IsFalse(results.Errors.HasWarnings);
			Assert.IsFalse(results.Errors.HasErrors);
		}

		public static Process SpawnMethod(Type type, string methodName, params string[] p)
		{
			string assemblyName = type.Assembly.FullName;
			string typename = type.FullName;
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("\"{0}\" \"{1}\" \"{2}\"", assemblyName, typename, methodName);
			foreach (string s in p)
			{
				sb.Append(" ");
				sb.Append("\"");
				sb.Append(s);
				sb.Append("\"");
			}

			Process proc = new Process();
			proc.StartInfo.Arguments = sb.ToString();
			proc.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Runner.exe");
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
			proc.StartInfo.RedirectStandardInput = false;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			proc.StartInfo.CreateNoWindow = true;
			proc.Start();
			return proc;
		}
	}
}
