
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
	public class GDCTests : NLogTestBase
	{
		[Test]
		public void GDCTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${gdc:item=myitem} ${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			GlobalDiagnosticsContext.Set("myitem", "myvalue");
			LogManager.GetLogger("A").Debug("a");
			AssertDebugLastMessage("debug", "myvalue a");

			GlobalDiagnosticsContext.Set("myitem", "value2");
			LogManager.GetLogger("A").Debug("b");
			AssertDebugLastMessage("debug", "value2 b");

			GlobalDiagnosticsContext.Remove("myitem");
			LogManager.GetLogger("A").Debug("c");
			AssertDebugLastMessage("debug", " c");
		}
	}
}