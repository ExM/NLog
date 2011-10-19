
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
			AssertDebugLastMessage("debug1", "a 1");
			LogManager.GetLogger("debug2").Debug("a");
			AssertDebugLastMessage("debug2", "a 1");
			LogManager.GetLogger("debug3").Debug("a");
			AssertDebugLastMessage("debug3", "a 2");
		}
	}
}
