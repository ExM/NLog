using NUnit.Framework;
using NLog.Config;
using System;
using System.IO;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace NLog.UnitTests
{
	[TestFixture]
	public class RegressionTests : NLogTestBase
	{
		[Test]
		public void Bug3990StackOverflowWhenUsingNLogViewerTarget()
		{
			// this would fail because of stack overflow in the 
			// constructor of NLogViewerTarget
			var config = CreateConfigurationFromString(@"
<nlog>
  <extensions> 
	<add assembly='NLog.Network'/> 
  </extensions>
  <targets>
	<target name='viewer' type='NLogViewer' address='udp://127.0.0.1:9999' />
  </targets>
  <rules>
	<logger name='*' minlevel='Debug' writeTo='viewer' />
  </rules>
</nlog>");

			var target = config.LoggingRules[0].Targets[0] as NLogViewerTarget;
			Assert.IsNotNull(target);
		}
	}
}