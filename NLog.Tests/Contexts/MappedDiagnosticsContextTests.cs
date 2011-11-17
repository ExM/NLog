using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using System.Text;

namespace NLog.UnitTests.Contexts
{
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
	}
}