using System;
using System.Xml;
using System.Reflection;
using NLog;
using NLog.Config;
using NUnit.Framework;

namespace NLog.UnitTests.LayoutRenderers
{
	[TestFixture]
	public class MDCTests : NLogTestBase
	{
		[Test]
		public void MDCTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${mdc:item=myitem} ${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			MappedDiagnosticsContext.Clear();
			MappedDiagnosticsContext.Set("myitem", "myvalue");
			LogManager.GetLogger("A").Debug("a");
			AssertDebugLastMessage("debug", "myvalue a");

			MappedDiagnosticsContext.Set("myitem", "value2");
			LogManager.GetLogger("A").Debug("b");
			AssertDebugLastMessage("debug", "value2 b");

			MappedDiagnosticsContext.Remove("myitem");
			LogManager.GetLogger("A").Debug("c");
			AssertDebugLastMessage("debug", " c");
		}
	}
}