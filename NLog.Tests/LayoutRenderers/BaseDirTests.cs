using System;
using System.Xml;
using System.Reflection;
using System.IO;
using NLog;
using NLog.Config;
using NUnit.Framework;
using NLog.Layouts;

namespace NLog.UnitTests.LayoutRenderers
{
	[TestFixture]
	public class BaseDirTests : NLogTestBase
	{
		private string baseDir = AppDomain.CurrentDomain.BaseDirectory;

		[Test]
		public void BaseDirTest()
		{
			AssertLayoutRendererOutput("${basedir}", baseDir);
		}

		[Test]
		public void BaseDirCombineTest()
		{
			AssertLayoutRendererOutput("${basedir:dir=aaa}", Path.Combine(baseDir, "aaa"));
		}

		[Test]
		public void BaseDirFileCombineTest()
		{
			AssertLayoutRendererOutput("${basedir:file=aaa.txt}", Path.Combine(baseDir, "aaa.txt"));
		}
	}
}
