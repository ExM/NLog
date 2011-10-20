
using System;
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

#if !SILVERLIGHT && !NET_CF

namespace NLog.UnitTests.LayoutRenderers
{
	using Microsoft.Win32;

	[TestFixture]
	public class RegistryTests : NLogTestBase
	{
		private const string TestKey = @"Software\NLogTest";

		[SetUp]
		public void Setup()
		{
			var key = Registry.CurrentUser.CreateSubKey(TestKey);
			key.SetValue("Foo", "FooValue");
			key.SetValue(null, "UnnamedValue");
		}

		[TearDown]
		public void TearDown()
		{
			Registry.CurrentUser.DeleteSubKey(TestKey);
		}

		[Test]
		public void RegistryNamedValueTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${registry:key=HKCU\\Software\\NLogTest:value=Foo}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			LogManager.GetLogger("d").Debug("zzz");
			AssertDebugLastMessage("debug", "FooValue");
		}

		[Test]
		public void RegistryUnnamedValueTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${registry:key=HKCU\\Software\\NLogTest}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			LogManager.GetLogger("d").Debug("zzz");
			AssertDebugLastMessage("debug", "UnnamedValue");

		}

		[Test]
		public void RegistryKeyNotFoundTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${registry:key=HKCU\\Software\\NoSuchKey:defaultValue=xyz}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			LogManager.GetLogger("d").Debug("zzz");
			AssertDebugLastMessage("debug", "xyz");
		}

		[Test]
		public void RegistryValueNotFoundTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${registry:key=HKCU\\Software\\NLogTest:value=NoSuchValue:defaultValue=xyz}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			LogManager.GetLogger("d").Debug("zzz");
			AssertDebugLastMessage("debug", "xyz");
		}
	}
}

#endif