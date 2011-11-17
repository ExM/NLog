using NUnit.Framework;
using NLog.Layouts;
using NLog.Filters;

namespace NLog.UnitTests.Filters
{
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
			Assert.IsInstanceOf<SimpleLayout>(wcf.Layout);
			Assert.AreEqual(((SimpleLayout)wcf.Layout).Text, "${message}");
			Assert.AreEqual(wcf.Substring, "zzz");
			Assert.AreEqual(FilterResult.Ignore, wcf.Action);
		}
	}
}