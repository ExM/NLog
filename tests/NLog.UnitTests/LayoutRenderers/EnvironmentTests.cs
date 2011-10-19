
#if !SILVERLIGHT && !NET_CF

using System;
using System.Xml;
using System.Reflection;

using NLog;
using NLog.Config;

using NUnit.Framework;

#if !NUNIT
	using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
	using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
	using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
	using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

namespace NLog.UnitTests.LayoutRenderers
{
	[TestFixture]
	public class EnvironmentTests : NLogTestBase
	{
		[Test]
		public void EnvironmentTest()
		{
			AssertLayoutRendererOutput("${environment:variable=PATH}", System.Environment.GetEnvironmentVariable("PATH"));
		}

		[Test]
		public void EnvironmentSimpleTest()
		{
			AssertLayoutRendererOutput("${environment:PATH}", System.Environment.GetEnvironmentVariable("PATH"));
		}
	}
}

#endif