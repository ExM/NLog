
namespace NLog.UnitTests.Config
{
	using System;
	using NUnit.Framework;
	using NLog.Config;
	using NLog.Internal;
	using NLog.Targets;
	using System.Collections.Generic;

	[TestFixture]
	public class ConfigurationItemFactoryTests : NLogTestBase
	{
		[Test]
		public void ConfigurationItemFactoryDefaultTest()
		{
			var cif = new ConfigurationItemFactory();
			Assert.IsInstanceOf<DebugTarget>(cif.CreateInstance(typeof(DebugTarget)));
		}

		[Test]
		public void ConfigurationItemFactorySimpleTest()
		{
			var cif = new ConfigurationItemFactory();
			cif.RegisterType(typeof(DebugTarget), string.Empty);
			var target = cif.Targets.CreateInstance("Debug") as DebugTarget;
			Assert.IsNotNull(target);
		}

		[Test]
		public void ConfigurationItemFactoryUsesSuppliedDelegateToResolveObject()
		{
			var cif = new ConfigurationItemFactory();
			cif.RegisterType(typeof(DebugTarget), string.Empty);
			List<Type> resolvedTypes = new List<Type>();
			cif.CreateInstance = t => { resolvedTypes.Add(t); return FactoryHelper.CreateInstance(t); };
			Target target = cif.Targets.CreateInstance("Debug");
			Assert.IsNotNull(target);
			Assert.AreEqual(1, resolvedTypes.Count);
			Assert.AreEqual(typeof(DebugTarget), resolvedTypes[0]);
		}

#if !SILVERLIGHT && !NET_CF
		// this is just to force reference to NLog.Extended.dll
		public Type ForceExtendedReference = typeof(MessageQueueTarget).DeclaringType;

		[Test]
		public void ExtendedTargetTest()
		{
			var targets = ConfigurationItemFactory.Default.Targets;

			AssertInstance(targets, "MSMQ", "MessageQueueTarget");
			AssertInstance(targets, "AspNetTrace", "AspNetTraceTarget");
			AssertInstance(targets, "AspNetBufferingWrapper", "AspNetBufferingTargetWrapper");
		}

		[Test]
		public void ExtendedLayoutRendererTest()
		{
			var layoutRenderers = ConfigurationItemFactory.Default.LayoutRenderers;

			AssertInstance(layoutRenderers, "aspnet-application", "AspNetApplicationValueLayoutRenderer");
			AssertInstance(layoutRenderers, "aspnet-request", "AspNetRequestValueLayoutRenderer");
			AssertInstance(layoutRenderers, "aspnet-sessionid", "AspNetSessionIDLayoutRenderer");
			AssertInstance(layoutRenderers, "aspnet-session", "AspNetSessionValueLayoutRenderer");
			AssertInstance(layoutRenderers, "aspnet-user-authtype", "AspNetUserAuthTypeLayoutRenderer");
			AssertInstance(layoutRenderers, "aspnet-user-identity", "AspNetUserIdentityLayoutRenderer");
		}

		private static void AssertInstance<T1, T2>(INamedItemFactory<T1, T2> targets, string itemName, string expectedTypeName)
			where T1 : class
		{
			Assert.AreEqual(expectedTypeName, targets.CreateInstance(itemName).GetType().Name);
		}
#endif
	}
}