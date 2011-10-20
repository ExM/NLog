
#pragma warning disable 0618

namespace NLog.UnitTests.Contexts
{
	using NUnit.Framework;

#if !NUNIT
	using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
	using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
	using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
	using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

	[TestFixture]
	public class GlobalDiagnosticsContextTests
	{
		[Test]
		public void GDCTest1()
		{
			GlobalDiagnosticsContext.Clear();
			Assert.IsFalse(GlobalDiagnosticsContext.Contains("foo"));
			Assert.AreEqual(string.Empty, GlobalDiagnosticsContext.Get("foo"));
			Assert.IsFalse(GlobalDiagnosticsContext.Contains("foo2"));
			Assert.AreEqual(string.Empty, GlobalDiagnosticsContext.Get("foo2"));

			GlobalDiagnosticsContext.Set("foo", "bar");
			GlobalDiagnosticsContext.Set("foo2", "bar2");

			Assert.IsTrue(GlobalDiagnosticsContext.Contains("foo"));
			Assert.AreEqual("bar", GlobalDiagnosticsContext.Get("foo"));

			GlobalDiagnosticsContext.Remove("foo");
			Assert.IsFalse(GlobalDiagnosticsContext.Contains("foo"));
			Assert.AreEqual(string.Empty, GlobalDiagnosticsContext.Get("foo"));

			Assert.IsTrue(GlobalDiagnosticsContext.Contains("foo2"));
			Assert.AreEqual("bar2", GlobalDiagnosticsContext.Get("foo2"));
		}

		[Test]
		public void GDCTest2()
		{
			GDC.Clear();
			Assert.IsFalse(GDC.Contains("foo"));
			Assert.AreEqual(string.Empty, GDC.Get("foo"));
			Assert.IsFalse(GDC.Contains("foo2"));
			Assert.AreEqual(string.Empty, GDC.Get("foo2"));

			GDC.Set("foo", "bar");
			GDC.Set("foo2", "bar2");

			Assert.IsTrue(GDC.Contains("foo"));
			Assert.AreEqual("bar", GDC.Get("foo"));

			GDC.Remove("foo");
			Assert.IsFalse(GDC.Contains("foo"));
			Assert.AreEqual(string.Empty, GDC.Get("foo"));

			Assert.IsTrue(GDC.Contains("foo2"));
			Assert.AreEqual("bar2", GDC.Get("foo2"));
		}
	}
}