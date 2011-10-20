
#if !NET_CF

using System;
using System.Globalization;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

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
	public class CallSiteTests : NLogTestBase
	{
#if !SILVERLIGHT
		[Test]
		public void LineNumberTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${callsite:filename=true} ${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
#line 100000
			logger.Debug("msg");
			string lastMessage = GetDebugLastMessage("debug");
			// There's a difference in handling line numbers between .NET and Mono
			// We're just interested in checking if it's above 100000
			Assert.IsTrue(lastMessage.IndexOf("callsitetests.cs:10000", StringComparison.OrdinalIgnoreCase) >= 0, "Invalid line number. Expected prefix of 10000, got: " + lastMessage);
#line default
		}
#endif

		[Test]
		public void MethodNameTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${callsite} ${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("msg");
			MethodBase currentMethod = MethodBase.GetCurrentMethod();
			AssertDebugLastMessage("debug", currentMethod.DeclaringType.FullName + "." + currentMethod.Name + " msg");
		}

		[Test]
		public void ClassNameTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=false} ${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("msg");
			MethodBase currentMethod = MethodBase.GetCurrentMethod();
			AssertDebugLastMessage("debug", currentMethod.DeclaringType.FullName + " msg");
		}

		[Test]
		public void ClassNameWithPaddingTestTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=false:padding=3:fixedlength=true} ${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("msg");
			MethodBase currentMethod = MethodBase.GetCurrentMethod();
			AssertDebugLastMessage("debug", currentMethod.DeclaringType.FullName.Substring(0, 3) + " msg");
		}

		[Test]
		public void MethodNameWithPaddingTestTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${callsite:classname=false:methodname=true:padding=16:fixedlength=true} ${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("msg");
			AssertDebugLastMessage("debug", "MethodNameWithPa msg");
		}
	}
}

#endif