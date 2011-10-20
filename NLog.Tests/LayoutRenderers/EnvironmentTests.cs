using System;
using System.Xml;
using System.Reflection;
using NLog;
using NLog.Config;
using NUnit.Framework;

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
