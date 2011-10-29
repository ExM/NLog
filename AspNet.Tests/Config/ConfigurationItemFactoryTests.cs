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
		[SetUp]
		public void LoadExtensionAssembly()
		{
			ConfigurationItemFactory.Default.RegisterItemsFromAssembly(typeof(MessageQueueTarget).Assembly);
		}

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
	}
}