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
			IsNumber(GetDebugLastMessage("debug"));
		}

		private void IsNumber(string text)
		{
			foreach (var ch in text)
				if (!Char.IsDigit(ch))
					Assert.Fail("`{0}' not a number", text);
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
			IsNumber(GetDebugLastMessage("debug"));
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
			IsNumber(GetDebugLastMessage("debug"));
		}
	}
}