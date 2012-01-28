using System;
using System.IO;
using NUnit.Framework;
using NLog.Filters;
using NLog.Layouts;
using NLog.Targets;
using NLog.Common;

namespace NLog.UnitTests.Config
{

	[TestFixture]
	public class ExtensionTests : NLogTestBase
	{
		private string extensionAssemblyName1 = "SampleExtensions";
		private string extensionAssemblyFullPath1 = Path.GetFullPath("SampleExtensions.dll");
		
		[Test]
		public void ExtensionTest1()
		{
			var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
	<extensions>
		<add assemblyFile='" + this.extensionAssemblyFullPath1 + @"' />
	</extensions>

	<targets>
		<target name='t' type='MyTarget' />
		<target name='d1' type='Debug' layout='${foo}' />
		<target name='d2' type='Debug'>
			<layout type='FooLayout' x='1'>
			</layout>
		</target>
	</targets>

	<rules>
	  <logger name='*' writeTo='t'>
		<filters>
		   <whenFoo x='44' action='Ignore' />
		</filters>
	  </logger>
	</rules>
</nlog>");

			Target myTarget = configuration.FindTargetByName("t");
			Assert.AreEqual("MyExtensionNamespace.MyTarget", myTarget.GetType().FullName);
			
			var d1Target = (DebugTarget)configuration.FindTargetByName("d1");
			var layout = d1Target.Layout as SimpleLayout;
			Assert.IsNotNull(layout);
			Assert.AreEqual(1, layout.Renderers.Count);
			Assert.AreEqual("MyExtensionNamespace.FooLayoutRenderer", layout.Renderers[0].GetType().FullName);

			var d2Target = (DebugTarget)configuration.FindTargetByName("d2");
			Assert.AreEqual("MyExtensionNamespace.FooLayout", d2Target.Layout.GetType().FullName);

			Assert.AreEqual(1, configuration.LoggingRules[0].Filters.Count);
			Assert.AreEqual("MyExtensionNamespace.WhenFooFilter", configuration.LoggingRules[0].Filters[0].GetType().FullName);
		}
		
		[Test]
		public void OverrideExtention()
		{
			var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
	<extensions>
		<add assemblyFile='" + Path.GetFullPath("SampleExtensions.dll") + @"' />
		<add assemblyFile='" + Path.GetFullPath("OverrideExtension.dll") + @"' />
	</extensions>

	<targets>
		<target name='t' type='MyTarget' />
	</targets>

	<rules>
	  <logger name='*' writeTo='t'/>
	</rules>
</nlog>");

			Target myTarget = configuration.FindTargetByName("t");
			Assert.AreEqual("MyOverrideExNamespace.MyTarget", myTarget.GetType().FullName);
		}
		
		[Test]
		public void OverrideDefault()
		{
			var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
	<extensions>
		<add assemblyFile='" + Path.GetFullPath("OverrideExtension.dll") + @"' />
	</extensions>

	<targets>
		<target name='t' type='File' />
	</targets>

	<rules>
	  <logger name='*' writeTo='t'/>
	</rules>
</nlog>");

			Target myTarget = configuration.FindTargetByName("t");
			Assert.AreEqual("MyOverrideExNamespace.MyFile", myTarget.GetType().FullName);
		}

		[Test]
		public void ExtensionTest2()
		{
			var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
	<extensions>
		<add assembly='" + this.extensionAssemblyName1 + @"' />
	</extensions>

	<targets>
		<target name='t' type='MyTarget' />
		<target name='d1' type='Debug' layout='${foo}' />
		<target name='d2' type='Debug'>
			<layout type='FooLayout' x='1'>
			</layout>
		</target>
	</targets>

	<rules>
	  <logger name='*' writeTo='t'>
		<filters>
		   <whenFoo x='44' action='Ignore' />
		   <when condition='myrandom(10)==3' action='Log' />
		</filters>
	  </logger>
	</rules>
</nlog>");

			Target myTarget = configuration.FindTargetByName("t");
			Assert.AreEqual("MyExtensionNamespace.MyTarget", myTarget.GetType().FullName);

			var d1Target = (DebugTarget)configuration.FindTargetByName("d1");
			var layout = d1Target.Layout as SimpleLayout;
			Assert.IsNotNull(layout);
			Assert.AreEqual(1, layout.Renderers.Count);
			Assert.AreEqual("MyExtensionNamespace.FooLayoutRenderer", layout.Renderers[0].GetType().FullName);

			var d2Target = (DebugTarget)configuration.FindTargetByName("d2");
			Assert.AreEqual("MyExtensionNamespace.FooLayout", d2Target.Layout.GetType().FullName);

			Assert.AreEqual(2, configuration.LoggingRules[0].Filters.Count);
			Assert.AreEqual("MyExtensionNamespace.WhenFooFilter", configuration.LoggingRules[0].Filters[0].GetType().FullName);
			var cbf = configuration.LoggingRules[0].Filters[1] as ConditionBasedFilter;
			Assert.IsNotNull(cbf);
			Assert.AreEqual("(myrandom(10) == 3)", cbf.Condition.ToString());
		}

		[Test]
		public void ExtensionWithPrefixTest()
		{
			var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
	<extensions>
		<add prefix='myprefix' assemblyFile='" + this.extensionAssemblyFullPath1 + @"' />
	</extensions>

	<targets>
		<target name='t' type='myprefix.MyTarget' />
		<target name='d1' type='Debug' layout='${myprefix.foo}' />
		<target name='d2' type='Debug'>
			<layout type='myprefix.FooLayout' x='1'>
			</layout>
		</target>
	</targets>

	<rules>
	  <logger name='*' writeTo='t'>
		<filters>
		   <myprefix.whenFoo x='44' action='Ignore' />
		</filters>
	  </logger>
	</rules>
</nlog>");

			Target myTarget = configuration.FindTargetByName("t");
			Assert.AreEqual("MyExtensionNamespace.MyTarget", myTarget.GetType().FullName);

			var d1Target = (DebugTarget)configuration.FindTargetByName("d1");
			var layout = d1Target.Layout as SimpleLayout;
			Assert.IsNotNull(layout);
			Assert.AreEqual(1, layout.Renderers.Count);
			Assert.AreEqual("MyExtensionNamespace.FooLayoutRenderer", layout.Renderers[0].GetType().FullName);

			var d2Target = (DebugTarget)configuration.FindTargetByName("d2");
			Assert.AreEqual("MyExtensionNamespace.FooLayout", d2Target.Layout.GetType().FullName);

			Assert.AreEqual(1, configuration.LoggingRules[0].Filters.Count);
			Assert.AreEqual("MyExtensionNamespace.WhenFooFilter", configuration.LoggingRules[0].Filters[0].GetType().FullName);
		}

		[Test]
		public void ExtensionTest4()
		{
			var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
	<extensions>
		<add type='MyExtensionNamespace.MyTarget, SampleExtensions, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' />
		<add type='MyExtensionNamespace.FooLayout, SampleExtensions, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' />
		<add type='MyExtensionNamespace.FooLayoutRenderer, SampleExtensions, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' />
		<add type='MyExtensionNamespace.WhenFooFilter, SampleExtensions, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' />
	</extensions>

	<targets>
		<target name='t' type='MyTarget' />
		<target name='d1' type='Debug' layout='${foo}' />
		<target name='d2' type='Debug'>
			<layout type='FooLayout' x='1'>
			</layout>
		</target>
	</targets>

	<rules>
	  <logger name='*' writeTo='t'>
		<filters>
		   <whenFoo x='44' action='Ignore' />
		</filters>
	  </logger>
	</rules>
</nlog>");

			Target myTarget = configuration.FindTargetByName("t");
			Assert.AreEqual("MyExtensionNamespace.MyTarget", myTarget.GetType().FullName);

			var d1Target = (DebugTarget)configuration.FindTargetByName("d1");
			var layout = d1Target.Layout as SimpleLayout;
			Assert.IsNotNull(layout);
			Assert.AreEqual(1, layout.Renderers.Count);
			Assert.AreEqual("MyExtensionNamespace.FooLayoutRenderer", layout.Renderers[0].GetType().FullName);

			var d2Target = (DebugTarget)configuration.FindTargetByName("d2");
			Assert.AreEqual("MyExtensionNamespace.FooLayout", d2Target.Layout.GetType().FullName);

			Assert.AreEqual(1, configuration.LoggingRules[0].Filters.Count);
			Assert.AreEqual("MyExtensionNamespace.WhenFooFilter", configuration.LoggingRules[0].Filters[0].GetType().FullName);
		}

		[Test]
		public void CustomLoad()
		{
			string winExt = Path.GetFullPath("NLog.WinTraits.dll");
			string unixExt = Path.GetFullPath("NLog.UnixTraits.dll");

			if (Platform.CurrentOS == PlatformID.Win32NT)
			{
				if(!File.Exists(winExt))
					Assert.Ignore("file {0} not found", winExt);
			}
			else if (Platform.CurrentOS == PlatformID.Unix)
			{
				if (!File.Exists(unixExt))
					Assert.Ignore("file {0} not found", unixExt);
			}
			else
				Assert.Ignore("unexpected OS: {0}", Platform.CurrentOS);


			var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
	<extensions>
		<add platform='Win32NT' assemblyFile='" + winExt + @"' />
		<add platform='Unix' assemblyFile='" + unixExt + @"' />
	</extensions>

	<targets>
		<target name='t' type='File' fileName='${basedir}\test.log' />
	</targets>

	<rules>
	  <logger name='*' writeTo='t'/>
	</rules>
</nlog>");

			Target myTarget = configuration.FindTargetByName("t");

			if (Platform.CurrentOS == PlatformID.Win32NT)
				Assert.AreEqual("NLog.WinTraits.Targets.FileTarget", myTarget.GetType().FullName);
			else //if (Platform.CurrentOS == PlatformID.Unix)
				Assert.AreEqual("NLog.UnixTraits.Targets.FileTarget", myTarget.GetType().FullName);
		}
		
        [Test]
        public void CustomXmlNamespaceTest()
        {
            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true' xmlns:foo='http://bar'>
    <targets>
        <target name='d' type='foo:Debug' />
    </targets>
</nlog>");

            var d1Target = (DebugTarget)configuration.FindTargetByName("d");
            Assert.IsNotNull(d1Target);
        }

	}
}
