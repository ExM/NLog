using System;
using System.Diagnostics;
using NLog;
using NUnit.Framework;
using System.Globalization;

namespace NLog.UnitTests.LayoutRenderers
{
	[TestFixture]
	public class ProcessInfoTests : NLogTestBase
	{
		[Test]
		public void DefaultProperty()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${processinfo}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			LogManager.GetLogger("d").Debug("zzz");
			AssertDebugLastMessage("debug",
				Convert.ToString(Process.GetCurrentProcess().Id, CultureInfo.InvariantCulture));
		}
		
		[Test]
		public void MainWindowHandle()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${processinfo:property=MainWindowHandle}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			LogManager.GetLogger("d").Debug("zzz");
			AssertDebugLastMessage("debug",
				Convert.ToString(Process.GetCurrentProcess().MainWindowHandle, CultureInfo.InvariantCulture));
		}
		
		[Test]
		public void VirtualMemorySize64()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${processinfo:property=VirtualMemorySize64}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			LogManager.GetLogger("d").Debug("zzz");
			AssertDebugLastMessage("debug",
				Convert.ToString(Process.GetCurrentProcess().VirtualMemorySize64, CultureInfo.InvariantCulture));
		}
	}
}