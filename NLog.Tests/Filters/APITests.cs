
namespace NLog.UnitTests.Filters
{
	using NUnit.Framework;

#if !NUNIT
	using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
	using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
	using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
	using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
	using NLog.Layouts;
	using NLog.Filters;

	[TestFixture]
	public class APITests : NLogTestBase
	{
		[Test]
		public void APITest()
		{
			// this is mostly to make Clover happy

			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug'>
					<filters>
						<whenContains layout='${message}' substring='zzz' action='Ignore' />
					</filters>
					</logger>
				</rules>
			</nlog>");

			Assert.IsTrue(LogManager.Configuration.LoggingRules[0].Filters[0] is WhenContainsFilter);
			var wcf = (WhenContainsFilter)LogManager.Configuration.LoggingRules[0].Filters[0];
			Assert.IsInstanceOfType(typeof(SimpleLayout), wcf.Layout);
			Assert.AreEqual(((SimpleLayout)wcf.Layout).Text, "${message}");
			Assert.AreEqual(wcf.Substring, "zzz");
			Assert.AreEqual(FilterResult.Ignore, wcf.Action);
		}
	}
}