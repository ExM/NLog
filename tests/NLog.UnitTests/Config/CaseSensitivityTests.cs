
namespace NLog.UnitTests.Config
{
	using NUnit.Framework;

#if !NUNIT
	using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
	using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
	using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
	using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

	[TestFixture]
	public class CaseSensitivityTests : NLogTestBase
	{
		[Test]
		public void LowerCaseTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='debug' layout='${message}' /></targets>
				<rules>
					<logger name='*' minlevel='info' appendto='debug'>
						<filters>
							<whencontains layout='${message}' substring='msg' action='ignore' />
						</filters>
					</logger>
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("msg");
			logger.Info("msg");
			logger.Warn("msg");
			logger.Error("msg");
			logger.Fatal("msg");
			logger.Debug("message");
			AssertDebugCounter("debug", 0);

			logger.Info("message");
			AssertDebugCounter("debug", 1);

			logger.Warn("message");
			AssertDebugCounter("debug", 2);

			logger.Error("message");
			AssertDebugCounter("debug", 3);

			logger.Fatal("message");
			AssertDebugCounter("debug", 4);
		}

		[Test]
		public void UpperCaseTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog throwExceptions='true'>
				<TARGETS><TARGET NAME='DEBUG' TYPE='DEBUG' LAYOUT='${MESSAGE}' /></TARGETS>
				<RULES>
					<LOGGER NAME='*' MINLEVEL='INFO' APPENDTO='DEBUG'>
						<FILTERS>
							<WHENCONTAINS LAYOUT='${MESSAGE}' SUBSTRING='msg' ACTION='IGNORE' />
						</FILTERS>
					</LOGGER>
				</RULES>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("msg");
			logger.Info("msg");
			logger.Warn("msg");
			logger.Error("msg");
			logger.Fatal("msg");
			logger.Debug("message");
			AssertDebugCounter("debug", 0);

			logger.Info("message");
			AssertDebugCounter("debug", 1);

			logger.Warn("message");
			AssertDebugCounter("debug", 2);

			logger.Error("message");
			AssertDebugCounter("debug", 3);

			logger.Fatal("message");
			AssertDebugCounter("debug", 4);
		}
	}
}