
namespace NLog.UnitTests.Config
{
	using System;
	using System.IO;
	using NUnit.Framework;

#if !NUNIT
	using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
	using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
	using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
	using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
	using MyExtensionNamespace;
	using NLog.Filters;
	using NLog.Layouts;
	using NLog.Targets;

	[TestFixture]
	public class ExtensionTests : NLogTestBase
	{
#if !WINDOWS_PHONE
		private string extensionAssemblyName1 = "SampleExtensions";
#if SILVERLIGHT || NET_CF
		private string extensionAssemblyFullPath1 = "SampleExtensions.dll";
#else
		private string extensionAssemblyFullPath1 = Path.GetFullPath("SampleExtensions.dll");
#endif
		
		[Test]
		public void ExtensionTest1()
		{
			Assert.IsNotNull(typeof(FooLayout));

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
#endif

		[Test]
		public void ExtensionTest4()
		{
			Assert.IsNotNull(typeof(FooLayout));

			var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
	<extensions>
		<add type='" + typeof(MyTarget).AssemblyQualifiedName + @"' />
		<add type='" + typeof(FooLayout).AssemblyQualifiedName + @"' />
		<add type='" + typeof(FooLayoutRenderer).AssemblyQualifiedName + @"' />
		<add type='" + typeof(WhenFooFilter).AssemblyQualifiedName + @"' />
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

	}
}
