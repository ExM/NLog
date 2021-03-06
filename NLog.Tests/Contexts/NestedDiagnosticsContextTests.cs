using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using System.Text;

namespace NLog.UnitTests.Contexts
{
	[TestFixture]
	public class NestedDiagnosticsContextTests
	{
		[Test]
		public void NDCTest1()
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
								NestedDiagnosticsContext.Clear();
								Assert.AreEqual(string.Empty, NestedDiagnosticsContext.TopMessage);
								Assert.AreEqual(string.Empty, NestedDiagnosticsContext.Pop());
								AssertContents(NestedDiagnosticsContext.GetAllMessages());
								using (NestedDiagnosticsContext.Push("foo"))
								{
									Assert.AreEqual("foo", NestedDiagnosticsContext.TopMessage);
									AssertContents(NestedDiagnosticsContext.GetAllMessages(), "foo");
									using (NestedDiagnosticsContext.Push("bar"))
									{
										AssertContents(NestedDiagnosticsContext.GetAllMessages(), "bar", "foo");
										Assert.AreEqual("bar", NestedDiagnosticsContext.TopMessage);
										NestedDiagnosticsContext.Push("baz");
										AssertContents(NestedDiagnosticsContext.GetAllMessages(), "baz", "bar", "foo");
										Assert.AreEqual("baz", NestedDiagnosticsContext.TopMessage);
										Assert.AreEqual("baz", NestedDiagnosticsContext.Pop());

										AssertContents(NestedDiagnosticsContext.GetAllMessages(), "bar", "foo");
										Assert.AreEqual("bar", NestedDiagnosticsContext.TopMessage);
									}

									AssertContents(NestedDiagnosticsContext.GetAllMessages(), "foo");
									Assert.AreEqual("foo", NestedDiagnosticsContext.TopMessage);
								}

								AssertContents(NestedDiagnosticsContext.GetAllMessages());
								Assert.AreEqual(string.Empty, NestedDiagnosticsContext.Pop());
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

		private static void AssertContents(string[] actual, params string[] expected)
		{
			Assert.AreEqual(expected.Length, actual.Length);
			for (int i = 0; i < expected.Length; ++i)
			{
				Assert.AreEqual(expected[i], actual[i]);
			}
		}
	}
}