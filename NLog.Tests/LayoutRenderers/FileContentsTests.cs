namespace NLog.UnitTests.LayoutRenderers
{
	using System;
	using System.IO;
	using System.Text;
	using NUnit.Framework;

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
