using System.Xml;
using NUnit.Framework;
using NLog.Config;

namespace NLog.UnitTests
{
	using System;
	using System.IO;
	using NLog.Layouts;
	using NLog.Targets;
	using NLog.Targets.Wrappers;

	[TestFixture]
	public class RegressionTests : NLogTestBase
	{
		[Test]
		public void Bug4655UnableToReconfigureExistingLoggers()
		{
			var debugTarget1 = new DebugTarget();
			var debugTarget2 = new DebugTarget();

			SimpleConfigurator.ConfigureForTargetLogging(debugTarget1, LogLevel.Debug);

			Logger logger = LogManager.GetLogger(Guid.NewGuid().ToString("N"));

			logger.Info("foo");

			Assert.AreEqual(1, debugTarget1.Counter);
			Assert.AreEqual(0, debugTarget2.Counter);

			LogManager.Configuration.AddTarget("DesktopConsole", debugTarget2);
			LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, debugTarget2));
			LogManager.ReconfigExistingLoggers();

			logger.Info("foo");

			Assert.AreEqual(2, debugTarget1.Counter);
			Assert.AreEqual(1, debugTarget2.Counter);
		}

		[Test]
		public void Bug5965StackOverflow()
		{
			LogManager.Configuration = this.CreateConfigurationFromString(@"
<nlog xmlns='http://www.nlog-project.org/schemas/NLog.xsd'
	  xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
  
  <targets>
	<target name='file'  xsi:type='AsyncWrapper' queueLimit='5000' overflowAction='Discard'  >
	  <target xsi:type='Debug'>
		<layout xsi:type='CSVLayout'>
		  <column name='counter' layout='${counter}' />
		  <column name='time' layout='${longdate}' />
		  <column name='message' layout='${message}' />
		</layout>
	  </target>
	</target>
  </targets>

  <rules>
	<logger name='*' minlevel='Trace' writeTo='file' />
  </rules>


</nlog>
");

			var log = LogManager.GetLogger("x");
			log.Fatal("Test");

			LogManager.Configuration = null;
		}
	}
}