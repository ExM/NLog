using NUnit.Framework;
using NLog.Config;
using NLog.Filters;

namespace NLog.UnitTests.Config
{
	[TestFixture]
	public class RuleConfigurationTests : NLogTestBase
	{
		[Test]
		public void NoRulesTest()
		{
			LoggingConfiguration c = CreateConfigurationFromString(@"
			<nlog>
				<targets>
					<target name='d1' type='Debug' />
				</targets>

				<rules>
				</rules>
			</nlog>");

			Assert.AreEqual(0, c.LoggingRules.Count);
		}

		[Test]
		public void SimpleRuleTest()
		{
			LoggingConfiguration c = CreateConfigurationFromString(@"
			<nlog>
				<targets>
					<target name='d1' type='Debug' />
				</targets>

				<rules>
					<logger name='*' minLevel='Info' writeTo='d1' />
				</rules>
			</nlog>");

			Assert.AreEqual(1, c.LoggingRules.Count);
			var rule = c.LoggingRules[0];
			Assert.AreEqual("*", rule.LoggerNamePattern);
			Assert.AreEqual(4, rule.Levels.Count);
			Assert.IsTrue(rule.Levels.Contains(LogLevel.Info));
			Assert.IsTrue(rule.Levels.Contains(LogLevel.Warn));
			Assert.IsTrue(rule.Levels.Contains(LogLevel.Error));
			Assert.IsTrue(rule.Levels.Contains(LogLevel.Fatal));
			Assert.AreEqual(1, rule.Targets.Count);
			Assert.AreSame(c.FindTargetByName("d1"), rule.Targets[0]);
			Assert.IsFalse(rule.Final);
			Assert.AreEqual(0, rule.Filters.Count);
		}

		[Test]
		public void SingleLevelTest()
		{
			LoggingConfiguration c = CreateConfigurationFromString(@"
			<nlog>
				<targets>
					<target name='d1' type='Debug' />
				</targets>

				<rules>
					<logger name='*' level='Warn' writeTo='d1' />
				</rules>
			</nlog>");

			Assert.AreEqual(1, c.LoggingRules.Count);
			var rule = c.LoggingRules[0];
			Assert.AreEqual(1, rule.Levels.Count);
			Assert.IsTrue(rule.Levels.Contains(LogLevel.Warn));
		}

		[Test]
		public void MinMaxLevelTest()
		{
			LoggingConfiguration c = CreateConfigurationFromString(@"
			<nlog>
				<targets>
					<target name='d1' type='Debug' />
				</targets>

				<rules>
					<logger name='*' minLevel='Info' maxLevel='Warn' writeTo='d1' />
				</rules>
			</nlog>");

			Assert.AreEqual(1, c.LoggingRules.Count);
			var rule = c.LoggingRules[0];
			Assert.AreEqual(2, rule.Levels.Count);
			Assert.IsTrue(rule.Levels.Contains(LogLevel.Info));
			Assert.IsTrue(rule.Levels.Contains(LogLevel.Warn));
		}

		[Test]
		public void NoLevelsTest()
		{
			LoggingConfiguration c = CreateConfigurationFromString(@"
			<nlog>
				<targets>
					<target name='d1' type='Debug' />
				</targets>

				<rules>
					<logger name='*' writeTo='d1' />
				</rules>
			</nlog>");

			Assert.AreEqual(1, c.LoggingRules.Count);
			var rule = c.LoggingRules[0];
			Assert.AreEqual(6, rule.Levels.Count);
			Assert.IsTrue(rule.Levels.Contains(LogLevel.Trace));
			Assert.IsTrue(rule.Levels.Contains(LogLevel.Debug));
			Assert.IsTrue(rule.Levels.Contains(LogLevel.Info));
			Assert.IsTrue(rule.Levels.Contains(LogLevel.Warn));
			Assert.IsTrue(rule.Levels.Contains(LogLevel.Error));
			Assert.IsTrue(rule.Levels.Contains(LogLevel.Fatal));
		}

		[Test]
		public void ExplicitLevelsTest()
		{
			LoggingConfiguration c = CreateConfigurationFromString(@"
			<nlog>
				<targets>
					<target name='d1' type='Debug' />
				</targets>

				<rules>
					<logger name='*' levels='Trace,Info,Warn' writeTo='d1' />
				</rules>
			</nlog>");

			Assert.AreEqual(1, c.LoggingRules.Count);
			var rule = c.LoggingRules[0];
			Assert.AreEqual(3, rule.Levels.Count);
			Assert.IsTrue(rule.Levels.Contains(LogLevel.Trace));
			Assert.IsTrue(rule.Levels.Contains(LogLevel.Info));
			Assert.IsTrue(rule.Levels.Contains(LogLevel.Warn));
		}

		[Test]
		public void MultipleTargetsTest()
		{
			LoggingConfiguration c = CreateConfigurationFromString(@"
			<nlog>
				<targets>
					<target name='d1' type='Debug' />
					<target name='d2' type='Debug' />
					<target name='d3' type='Debug' />
					<target name='d4' type='Debug' />
				</targets>

				<rules>
					<logger name='*' level='Warn' writeTo='d1,d2,d3' />
				</rules>
			</nlog>");

			Assert.AreEqual(1, c.LoggingRules.Count);
			var rule = c.LoggingRules[0];
			Assert.AreEqual(3, rule.Targets.Count);
			Assert.AreSame(c.FindTargetByName("d1"), rule.Targets[0]);
			Assert.AreSame(c.FindTargetByName("d2"), rule.Targets[1]);
			Assert.AreSame(c.FindTargetByName("d3"), rule.Targets[2]);
		}

		[Test]
		public void MultipleRulesSameTargetTest()
		{
			LoggingConfiguration c = CreateConfigurationFromString(@"
			<nlog>
				<targets>
					<target name='d1' type='Debug' layout='${message}' />
					<target name='d2' type='Debug' layout='${message}' />
					<target name='d3' type='Debug' layout='${message}' />
					<target name='d4' type='Debug' layout='${message}' />
				</targets>

				<rules>
					<logger name='*' level='Warn' writeTo='d1' />
					<logger name='*' level='Warn' writeTo='d2' />
					<logger name='*' level='Warn' writeTo='d3' />
				</rules>
			</nlog>");

			LogFactory factory = new LogFactory(c);
			var loggerConfig = factory.GetConfigurationForLogger("AAA", c);
			var targets = loggerConfig.GetTargetsForLevel(LogLevel.Warn);
			Assert.AreEqual("d1", targets.Target.Name);
			Assert.AreEqual("d2", targets.NextInChain.Target.Name);
			Assert.AreEqual("d3", targets.NextInChain.NextInChain.Target.Name);
			Assert.IsNull(targets.NextInChain.NextInChain.NextInChain);

			LogManager.Configuration = c;

			var logger = LogManager.GetLogger("BBB");
			logger.Warn("test1234");

			this.AssertDebugLastMessage("d1", "test1234");
			this.AssertDebugLastMessage("d2", "test1234");
			this.AssertDebugLastMessage("d3", "test1234");
			this.AssertDebugLastMessage("d4", string.Empty);
		}

		[Test]
		public void ChildRulesTest()
		{
			LoggingConfiguration c = CreateConfigurationFromString(@"
			<nlog>
				<targets>
					<target name='d1' type='Debug' />
					<target name='d2' type='Debug' />
					<target name='d3' type='Debug' />
					<target name='d4' type='Debug' />
				</targets>

				<rules>
					<logger name='*' level='Warn' writeTo='d1,d2,d3'>
						<logger name='Foo*' writeTo='d4' />
						<logger name='Bar*' writeTo='d4' />
					</logger>
				</rules>
			</nlog>");

			Assert.AreEqual(1, c.LoggingRules.Count);
			var rule = c.LoggingRules[0];
			Assert.AreEqual(2, rule.ChildRules.Count);
			Assert.AreEqual("Foo*", rule.ChildRules[0].LoggerNamePattern);
			Assert.AreEqual("Bar*", rule.ChildRules[1].LoggerNamePattern);
		}

		[Test]
		public void FiltersTest()
		{
			LoggingConfiguration c = CreateConfigurationFromString(@"
			<nlog>
				<targets>
					<target name='d1' type='Debug' />
					<target name='d2' type='Debug' />
					<target name='d3' type='Debug' />
					<target name='d4' type='Debug' />
				</targets>

				<rules>
					<logger name='*' level='Warn' writeTo='d1,d2,d3'>
						<filters>
							<when condition=""starts-with(message, 'x')"" action='Ignore' />
							<when condition=""starts-with(message, 'z')"" action='Ignore' />
						</filters>
					</logger>
				</rules>
			</nlog>");

			Assert.AreEqual(1, c.LoggingRules.Count);
			var rule = c.LoggingRules[0];
			Assert.AreEqual(2, rule.Filters.Count);
			var conditionBasedFilter = rule.Filters[0] as ConditionBasedFilter;
			Assert.IsNotNull(conditionBasedFilter);
			Assert.AreEqual("starts-with(message, 'x')", conditionBasedFilter.Condition.ToString());
			Assert.AreEqual(FilterResult.Ignore, conditionBasedFilter.Action);

			conditionBasedFilter = rule.Filters[1] as ConditionBasedFilter;
			Assert.IsNotNull(conditionBasedFilter);
			Assert.AreEqual("starts-with(message, 'z')", conditionBasedFilter.Condition.ToString());
			Assert.AreEqual(FilterResult.Ignore, conditionBasedFilter.Action);
		}
	}
}