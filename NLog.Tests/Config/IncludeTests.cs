
namespace NLog.UnitTests.Config
{
	using System;
	using System.IO;
	using NUnit.Framework;
	using NLog.Config;

	[TestFixture]
	public class IncludeTests : NLogTestBase
	{
		[Test]
		public void IncludeTest()
		{
			string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempPath);

			using (StreamWriter fs = File.CreateText(Path.Combine(tempPath, "included.nlog")))
			{
				fs.Write(@"<nlog>
					<targets><target name='debug' type='Debug' layout='${message}' /></targets>
			</nlog>");
			}

			using (StreamWriter fs = File.CreateText(Path.Combine(tempPath, "main.nlog")))
			{
				fs.Write(@"<nlog>
				<include file='included.nlog' />
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");
			}

			string fileToLoad = Path.Combine(tempPath, "main.nlog");

			try
			{
				// load main.nlog from the XAP
				LogManager.Configuration = new XmlLoggingConfiguration(fileToLoad);

				LogManager.GetLogger("A").Debug("aaa");
				AssertDebugLastMessage("debug", "aaa");
			}
			finally
			{
				LogManager.Configuration = null;
				if (Directory.Exists(tempPath))
					Directory.Delete(tempPath, true);
			}
		}

		[Test]
		[ExpectedException(typeof(NLogConfigurationException))]
		public void IncludeNotExistingTest()
		{
			string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempPath);

			using (StreamWriter fs = File.CreateText(Path.Combine(tempPath, "main.nlog")))
			{
				fs.Write(@"<nlog>
				<include file='included.nlog' />
			</nlog>");
			}

			string fileToLoad = Path.Combine(tempPath, "main.nlog");

			try
			{
				new XmlLoggingConfiguration(fileToLoad);
			}
			finally
			{
				if (Directory.Exists(tempPath))
					Directory.Delete(tempPath, true);
			}
		}

		[Test]
		public void IncludeNotExistingIgnoredTest()
		{
			string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempPath);

			using (StreamWriter fs = File.CreateText(Path.Combine(tempPath, "main.nlog")))
			{
				fs.Write(@"<nlog>
				<include file='included-notpresent.nlog' ignoreErrors='true' />
				<targets><target name='debug' type='Debug' layout='${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");
			}

			string fileToLoad = Path.Combine(tempPath, "main.nlog");
			try
			{
				LogManager.Configuration = new XmlLoggingConfiguration(fileToLoad);
				LogManager.GetLogger("A").Debug("aaa");
				AssertDebugLastMessage("debug", "aaa");
			}
			finally
			{
				LogManager.Configuration = null;
				if (Directory.Exists(tempPath))
					Directory.Delete(tempPath, true);
			}
		}
	}
}