
#pragma warning disable 0618

namespace NLog.UnitTests.Contexts
{
	using NUnit.Framework;

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
	}
}