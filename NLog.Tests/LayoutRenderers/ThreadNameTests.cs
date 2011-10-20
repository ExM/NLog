
using System;
using System.Diagnostics;
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