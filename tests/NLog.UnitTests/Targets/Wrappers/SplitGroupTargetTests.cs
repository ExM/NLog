
namespace NLog.UnitTests.Targets.Wrappers
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
	using NLog.Common;
	using NLog.Internal;
	using NLog.Targets;
	using NLog.Targets.Wrappers;

	[TestFixture]
	public class SplitGroupTargetTests : NLogTestBase
	{
		[Test]
		public void SplitGroupSyncTest1()
		{
			var myTarget1 = new MyTarget();
			var myTarget2 = new MyTarget();
			var myTarget3 = new MyTarget();

			var wrapper = new SplitGroupTarget()
			{
				Targets = { myTarget1, myTarget2, myTarget3 },
			};

			myTarget1.Initialize(null);
			myTarget2.Initialize(null);
			myTarget3.Initialize(null);
			wrapper.Initialize(null);

			List<Exception> exceptions = new List<Exception>();

			var inputEvents = new List<LogEventInfo>();
			for (int i = 0; i < 10; ++i)
			{
				inputEvents.Add(LogEventInfo.CreateNullEvent());
			}

			int remaining = inputEvents.Count;
			var allDone = new ManualResetEvent(false);

			// no exceptions
			for (int i = 0; i < inputEvents.Count; ++i)
			{
				wrapper.WriteAsyncLogEvent(inputEvents[i].WithContinuation(ex =>
					{
						lock (exceptions)
						{
							exceptions.Add(ex);
							if (Interlocked.Decrement(ref remaining) == 0)
							{
								allDone.Set();
							}
						};
					}));
			}

			allDone.WaitOne();

			Assert.AreEqual(inputEvents.Count, exceptions.Count);
			foreach (var e in exceptions)
			{
				Assert.IsNull(e);
			}

			Assert.AreEqual(inputEvents.Count, myTarget1.WriteCount);
			Assert.AreEqual(inputEvents.Count, myTarget2.WriteCount);
			Assert.AreEqual(inputEvents.Count, myTarget3.WriteCount);

			for (int i = 0; i < inputEvents.Count; ++i)
			{
				Assert.AreSame(inputEvents[i], myTarget1.WrittenEvents[i]);
				Assert.AreSame(inputEvents[i], myTarget2.WrittenEvents[i]);
				Assert.AreSame(inputEvents[i], myTarget3.WrittenEvents[i]);
			}

			Exception flushException = null;
			var flushHit = new ManualResetEvent(false);
			wrapper.Flush(ex => { flushException = ex; flushHit.Set(); });

			flushHit.WaitOne();
			if (flushException != null)
			{
				Assert.Fail(flushException.ToString());
			}

			Assert.AreEqual(1, myTarget1.FlushCount);
			Assert.AreEqual(1, myTarget2.FlushCount);
			Assert.AreEqual(1, myTarget3.FlushCount);
		}

		[Test]
		public void SplitGroupSyncTest2()
		{
			var wrapper = new SplitGroupTarget()
			{
				// no targets
			};

			wrapper.Initialize(null);

			List<Exception> exceptions = new List<Exception>();

			// no exceptions
			for (int i = 0; i < 10; ++i)
			{
				wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
			}

			Assert.AreEqual(10, exceptions.Count);
			foreach (var e in exceptions)
			{
				Assert.IsNull(e);
			}

			Exception flushException = new Exception("Flush not hit synchronously.");
			wrapper.Flush(ex => flushException = ex);

			if (flushException != null)
			{
				Assert.Fail(flushException.ToString());
			}
		}

		public class MyAsyncTarget : Target
		{
			public int FlushCount { get; private set; }
			public int WriteCount { get; private set; }

			protected override void Write(LogEventInfo logEvent)
			{
				throw new NotSupportedException();
			}

			protected override void Write(AsyncLogEventInfo logEvent)
			{
				Assert.IsTrue(this.FlushCount <= this.WriteCount);
				this.WriteCount++;
				ThreadPool.QueueUserWorkItem(
					s =>
						{
							if (this.ThrowExceptions)
							{
								logEvent.Continuation(new InvalidOperationException("Some problem!"));
								logEvent.Continuation(new InvalidOperationException("Some problem!"));
							}
							else
							{
								logEvent.Continuation(null);
								logEvent.Continuation(null);
							}
						});
			}

			protected override void FlushAsync(AsyncContinuation asyncContinuation)
			{
				this.FlushCount++;
				ThreadPool.QueueUserWorkItem(
					s => asyncContinuation(null));
			}

			public bool ThrowExceptions { get; set; }
		}

		public class MyTarget : Target
		{
			public MyTarget()
			{
				this.WrittenEvents = new List<LogEventInfo>();
			}

			public int FlushCount { get; set; }
			public int WriteCount { get; set; }
			public int FailCounter { get; set; }
			public List<LogEventInfo> WrittenEvents { get; private set; }

			protected override void Write(LogEventInfo logEvent)
			{
				Assert.IsTrue(this.FlushCount <= this.WriteCount);
				lock (this)
				{
					this.WriteCount++;
					this.WrittenEvents.Add(logEvent);
				}

				if (this.FailCounter > 0)
				{
					this.FailCounter--;
					throw new InvalidOperationException("Some failure.");
				}
			}

			protected override void FlushAsync(AsyncContinuation asyncContinuation)
			{
				this.FlushCount++;
				asyncContinuation(null);
			}
		}
	}
}
