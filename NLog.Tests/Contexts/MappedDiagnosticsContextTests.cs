
using System.Text;

#pragma warning disable 0618

namespace NLog.UnitTests.Contexts
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using NUnit.Framework;

#if !NUNIT
	using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
	using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
	using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
	using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

	[TestFixture]
	public class MappedDiagnosticsContextTests
	{
		/// <summary>
		/// Same as <see cref="MappedDiagnosticsContext" />, but there is one <see cref="MappedDiagnosticsContext"/> per each thread.
		/// </summary>
		[Test]
		public void MDCTest1()
		{
			List<Exception> exceptions = new List<Exception>();
			ManualResetEvent mre = new ManualResetEvent(false);
			int counter = 100;
			int remaining = counter;

			for (int i = 0; i < counter; ++i)
			{
				ThreadPool.QueueUserWorkItem(
					s =>
						{
							try
							{
								MappedDiagnosticsContext.Clear();
								Assert.IsFalse(MappedDiagnosticsContext.Contains("foo"), "#1");
								Assert.AreEqual(string.Empty, MappedDiagnosticsContext.Get("foo"), "#2");
								Assert.IsFalse(MappedDiagnosticsContext.Contains("foo2"), "#3");
								Assert.AreEqual(string.Empty, MappedDiagnosticsContext.Get("foo2"), "#4");

								MappedDiagnosticsContext.Set("foo", "bar");
								MappedDiagnosticsContext.Set("foo2", "bar2");

								Assert.IsTrue(MappedDiagnosticsContext.Contains("foo"));
								Assert.AreEqual("bar", MappedDiagnosticsContext.Get("foo"));

								MappedDiagnosticsContext.Remove("foo");
								Assert.IsFalse(MappedDiagnosticsContext.Contains("foo"));
								Assert.AreEqual(string.Empty, MappedDiagnosticsContext.Get("foo"));

								Assert.IsTrue(MappedDiagnosticsContext.Contains("foo2"));
								Assert.AreEqual("bar2", MappedDiagnosticsContext.Get("foo2"));
							}
							catch (Exception exception)
							{
								lock (exceptions)
								{
									exceptions.Add(exception);
								}
							}
							finally
							{
								if (Interlocked.Decrement(ref remaining) == 0)
								{
									mre.Set();
								}
							}
						});
			}

			mre.WaitOne();
			StringBuilder exceptionsMessage = new StringBuilder();
			foreach (var ex in exceptions)
			{
				if (exceptionsMessage.Length > 0)
				{
					exceptionsMessage.Append("\r\n");
				}

				exceptionsMessage.Append(ex.ToString());
			}

			Assert.AreEqual(0, exceptions.Count, exceptionsMessage.ToString());
		}

		[Test]
		public void MDCTest2()
		{
			List<Exception> exceptions = new List<Exception>();
			ManualResetEvent mre = new ManualResetEvent(false);
			int counter = 100;
			int remaining = counter;

			for (int i = 0; i < counter; ++i)
			{
				ThreadPool.QueueUserWorkItem(
					s =>
					{
						try
						{
							MDC.Clear();
							Assert.IsFalse(MDC.Contains("foo"));
							Assert.AreEqual(string.Empty, MDC.Get("foo"));
							Assert.IsFalse(MDC.Contains("foo2"));
							Assert.AreEqual(string.Empty, MDC.Get("foo2"));

							MDC.Set("foo", "bar");
							MDC.Set("foo2", "bar2");

							Assert.IsTrue(MDC.Contains("foo"));
							Assert.AreEqual("bar", MDC.Get("foo"));

							MDC.Remove("foo");
							Assert.IsFalse(MDC.Contains("foo"));
							Assert.AreEqual(string.Empty, MDC.Get("foo"));

							Assert.IsTrue(MDC.Contains("foo2"));
							Assert.AreEqual("bar2", MDC.Get("foo2"));
						}
						catch (Exception ex)
						{
							lock (exceptions)
							{
								exceptions.Add(ex);
							}
						}
						finally
						{
							if (Interlocked.Decrement(ref remaining) == 0)
							{
								mre.Set();
							}
						}
					});
			}

			mre.WaitOne();
			StringBuilder exceptionsMessage = new StringBuilder();
			foreach (var ex in exceptions)
			{
				if (exceptionsMessage.Length > 0)
				{
					exceptionsMessage.Append("\r\n");
				}

				exceptionsMessage.Append(ex.ToString());
			}

			Assert.AreEqual(0, exceptions.Count, exceptionsMessage.ToString());
		}
	}
}