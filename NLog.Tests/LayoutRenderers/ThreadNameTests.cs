using System;
using System.Diagnostics;
using System.Xml;
using System.Reflection;
using NLog;
using NLog.Config;
using NUnit.Framework;

namespace NLog.UnitTests.LayoutRenderers
{
	[TestFixture]
	public class ThreadNameTests : NLogTestBase
	{
		[Test]
		public void ThreadNameTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${threadname} ${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			if (System.Threading.Thread.CurrentThread.Name == null)
				System.Threading.Thread.CurrentThread.Name = "mythreadname";

			LogManager.GetLogger("A").Debug("a");
			AssertDebugLastMessage("debug", System.Threading.Thread.CurrentThread.Name + " a");
		}
	}
}