
#if !SILVERLIGHT && !NET_CF

using System;
using System.Xml;
using System.Reflection;
using System.IO;

using NLog;
using NLog.Config;

using NUnit.Framework;

#if !NUNIT
	using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
	using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
	using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
	using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
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

#endif