
namespace NLog.UnitTests.Config
{
	using System;
	using System.IO;
	using NUnit.Framework;

#if !NUNIT
	using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
	using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
	using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
	using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
	using ExpectedException = Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedExceptionAttribute;
#endif
	using NLog.Config;

	[TestFixture]
	public class IncludeTests : NLogTestBase
	{
		[Test]
		public void IncludeTest()
		{
#if SILVERLIGHT
			// file is pre-packaged in the XAP
			string fileToLoad = "ConfigFiles/main.nlog";
#else
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
#endif
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
#if !SILVERLIGHT
				if (Directory.Exists(tempPath))
					Directory.Delete(tempPath, true);
#endif
			}
		}

		[Test]
		[ExpectedException(typeof(NLogConfigurationException))]
		public void IncludeNotExistingTest()
		{
#if SILVERLIGHT
			string fileToLoad = "ConfigFiles/referencemissingfile.nlog";
#else
			string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempPath);

			using (StreamWriter fs = File.CreateText(Path.Combine(tempPath, "main.nlog")))
			{
				fs.Write(@"<nlog>
				<include file='included.nlog' />
			</nlog>");
			}

			string fileToLoad = Path.Combine(tempPath, "main.nlog");

#endif
			try
			{
				new XmlLoggingConfiguration(fileToLoad);
			}
			finally
			{
#if !SILVERLIGHT
				if (Directory.Exists(tempPath))
					Directory.Delete(tempPath, true);
#endif
			}
		}

		[Test]
		public void IncludeNotExistingIgnoredTest()
		{
#if SILVERLIGHT
			string fileToLoad = "ConfigFiles/referencemissingfileignored.nlog";
#else
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
#endif
			try
			{
				LogManager.Configuration = new XmlLoggingConfiguration(fileToLoad);
				LogManager.GetLogger("A").Debug("aaa");
				AssertDebugLastMessage("debug", "aaa");
			}
			finally
			{
				LogManager.Configuration = null;
#if !SILVERLIGHT
				if (Directory.Exists(tempPath))
					Directory.Delete(tempPath, true);
#endif
			}
		}
	}
}