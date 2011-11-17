using NUnit.Framework;
using NLog.LayoutRenderers;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.UnitTests.Config
{
	[TestFixture]
	public class VariableTests : NLogTestBase
	{
		[Test]
		public void VariablesTest1()
		{
			var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
	<variable name='prefix' value='[[' />
	<variable name='suffix' value=']]' />

	<targets>
		<target name='d1' type='Debug' layout='${prefix}${message}${suffix}' />
	</targets>
</nlog>");

			var d1 = configuration.FindTargetByName("d1") as DebugTarget;
			Assert.IsNotNull(d1);
			var layout = d1.Layout as SimpleLayout;
			Assert.IsNotNull(layout);
			Assert.AreEqual(3, layout.Renderers.Count);
			var lr1 = layout.Renderers[0] as LiteralLayoutRenderer;
			var lr2 = layout.Renderers[1] as MessageLayoutRenderer;
			var lr3 = layout.Renderers[2] as LiteralLayoutRenderer;
			Assert.IsNotNull(lr1);
			Assert.IsNotNull(lr2);
			Assert.IsNotNull(lr3);
			Assert.AreEqual("[[", lr1.Text);
			Assert.AreEqual("]]", lr3.Text);
		}
	}
}