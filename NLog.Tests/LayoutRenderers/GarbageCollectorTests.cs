using System;
using System.Diagnostics;
using NLog;
using NUnit.Framework;
using System.Globalization;

namespace NLog.UnitTests.LayoutRenderers
{
	[TestFixture]
	public class GarbageCollectorTests : NLogTestBase
	{
		[Test]
		public void DefaultProperty()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${gc}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			LogManager.GetLogger("d").Debug("zzz");
			AssertDebugLastMessage("debug",
				Convert.ToString(GC.GetTotalMemory(false), CultureInfo.InvariantCulture));
		}
		
		[Test]
		public void CollectionCount2()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${gc:property=CollectionCount2}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			LogManager.GetLogger("d").Debug("zzz");
			AssertDebugLastMessage("debug",
				Convert.ToString(GC.CollectionCount(2), CultureInfo.InvariantCulture));
		}
		
		[Test]
		public void MaxGeneration()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${gc:property=MaxGeneration}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			LogManager.GetLogger("d").Debug("zzz");
			AssertDebugLastMessage("debug",
				Convert.ToString(GC.MaxGeneration, CultureInfo.InvariantCulture));
		}
	}
}