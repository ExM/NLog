
#if !SILVERLIGHT

namespace NLog.UnitTests.LayoutRenderers
{
	using System;
	using System.IO;
	using System.Text;
	using NUnit.Framework;

#if !NUNIT
	using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
	using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
	using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
	using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

	[TestFixture]
	public class FileContentsTests : NLogTestBase
	{
		[Test]
		public void FileContentUnicodeTest()
		{
			string content = "12345";
			string fileName = Guid.NewGuid().ToString("N") + ".txt";
			using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.Unicode))
			{
				sw.Write(content);
			}

			this.AssertLayoutRendererOutput("${file-contents:" + fileName + ":encoding=utf-16}", content);
		}

		[Test]
		public void FileContentUTF8Test()
		{
			string content = "12345";
			string fileName = Guid.NewGuid().ToString("N") + ".txt";
			using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
			{
				sw.Write(content);
			}

			this.AssertLayoutRendererOutput("${file-contents:" + fileName + ":encoding=utf-8}", content);
		}

		[Test]
		public void FileContentTest2()
		{
			this.AssertLayoutRendererOutput("${file-contents:nosuchfile.txt}", string.Empty);
		}
	}
}

#endif