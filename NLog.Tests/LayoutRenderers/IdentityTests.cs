namespace NLog.UnitTests.LayoutRenderers
{
	using NUnit.Framework;
	using System.Security.Principal;
	using System.Threading;

	[TestFixture]
	public class IdentityTests : NLogTestBase
	{
		[Test]
		public void IdentityTest1()
		{
			var oldPrincipal = Thread.CurrentPrincipal;

			Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("SOMEDOMAIN\\SomeUser", "CustomAuth"), new[] { "Role1", "Role2" });
			try
			{
				AssertLayoutRendererOutput("${identity}", "auth:CustomAuth:SOMEDOMAIN\\SomeUser");
				AssertLayoutRendererOutput("${identity:authtype=false}", "auth:SOMEDOMAIN\\SomeUser");
				AssertLayoutRendererOutput("${identity:authtype=false:isauthenticated=false}", "SOMEDOMAIN\\SomeUser");
				AssertLayoutRendererOutput("${identity:fsnormalize=true}", "auth_CustomAuth_SOMEDOMAIN_SomeUser");
			}
			finally 
			{
				Thread.CurrentPrincipal = oldPrincipal;
			}
		}

		[Test]
		public void IdentityTest2()
		{
			AssertLayoutRendererOutput("${identity}", "notauth::");
		}
	}
}
