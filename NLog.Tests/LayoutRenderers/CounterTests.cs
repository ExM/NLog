using System;
using System.Xml;
using System.Reflection;
using NLog;
using NLog.Config;
using NUnit.Framework;

namespace NLog.UnitTests.LayoutRenderers
{
	[TestFixture]
	public class CounterTests : NLogTestBase
	{
		[Test]
		public void DefaultCounterTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message} ${counter} ${counter}' /></targets>
				<rules>
					<logger name='*' minlevel='Info' writeTo='debug' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			logger.Info("a");
			AssertDebugLastMessage("debug", "a 1 1");
			logger.Warn("a");
			AssertDebugLastMessage("debug", "a 2 2");
			logger.Error("a");
			AssertDebugLastMessage("debug", "a 3 3");
			logger.Fatal("a");
			AssertDebugLastMessage("debug", "a 4 4");
		}

		[Test]
		public void PresetCounterTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message} ${counter:value=1:increment=3} ${counter}' /></targets>
				<rules>
					<logger name='*' minlevel='Info' writeTo='debug' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			logger.Info("a");
			AssertDebugLastMessage("debug", "a 1 1");
			logger.Warn("a");
			AssertDebugLastMessage("debug", "a 4 2");
			logger.Error("a");
			AssertDebugLastMessage("debug", "a 7 3");
			logger.Fatal("a");
			AssertDebugLastMessage("debug", "a 10 4");
		}

		private static int staticCounterA = 0;
		private static int staticCounterB = 0;

		[Test]
		public void NamedCounterTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets>
					<target name='debug1' type='Debug' layout='${message} ${counter:sequence=aaa}' />
					<target name='debug2' type='Debug' layout='${message} ${counter:sequence=bbb}' />
					<target name='debug3' type='Debug' layout='${message} ${counter:sequence=aaa}' />
				</targets>
				<rules>
					<logger name='debug1' minlevel='Debug' writeTo='debug1' />
					<logger name='debug2' minlevel='Debug' writeTo='debug2' />
					<logger name='debug3' minlevel='Debug' writeTo='debug3' />
				</rules>
			</nlog>");

			LogManager.GetLogger("debug1").Debug("a");
			staticCounterA++;

			AssertDebugLastMessage("debug1", "a " + staticCounterA.ToString());

			LogManager.GetLogger("debug2").Debug("a");
			staticCounterB++;

			AssertDebugLastMessage("debug2", "a " + staticCounterB.ToString());

			LogManager.GetLogger("debug3").Debug("a");
			staticCounterA++;

			AssertDebugLastMessage("debug3", "a " + staticCounterA.ToString());
		}
	}
}
