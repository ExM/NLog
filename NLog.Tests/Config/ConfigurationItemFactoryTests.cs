using System;
using NUnit.Framework;
using NLog.Config;
using NLog.Internal;
using NLog.Targets;
using System.Collections.Generic;

namespace NLog.UnitTests.Config
{
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
			Func<Type, object> standartCreater = cif.CreateInstance;
			cif.CreateInstance = t => { resolvedTypes.Add(t); return standartCreater(t); };
			Target target = cif.Targets.CreateInstance("Debug");
			
			Assert.IsNotNull(target);
			Assert.AreEqual(1, resolvedTypes.Count);
			Assert.AreEqual(typeof(DebugTarget), resolvedTypes[0]);
		}

		private static void AssertInstance<T1, T2>(INamedItemFactory<T1, T2> targets, string itemName, string expectedTypeName)
			where T1 : class
		{
			Assert.AreEqual(expectedTypeName, targets.CreateInstance(itemName).GetType().Name);
		}
	}
}