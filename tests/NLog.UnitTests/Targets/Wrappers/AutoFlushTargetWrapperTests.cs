
namespace NLog.UnitTests.Targets.Wrappers
{
	using System;
	using System.Threading;
	using NUnit.Framework;

#if !NUNIT
	using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
	using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
	using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
	using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
	using NLog.Common;
	using NLog.Targets;
	using NLog.Targets.Wrappers;

	using NLog.Internal;

	[TestFixture]
	public class AutoFlushTargetWrapperTests : NLogTestBase
	{
		[Test]
		public void AutoFlushTargetWrapperSyncTest1()
		{
			var myTarget = new MyTarget();
			var wrapper = new AutoFlushTargetWrapper
			{
				WrappedTarget = myTarget,
			};

			myTarget.Initialize(null);
			wrapper.Initialize(null);
			var logEvent = new LogEventInfo();
			Exception lastException = null;
			bool continuationHit = false;
			AsyncContinuation continuation =
				ex =>
					{
						lastException = ex;
						continuationHit = true;
					};

			wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));

			Assert.IsTrue(continuationHit);
			Assert.IsNull(lastException);
			Assert.AreEqual(1, myTarget.FlushCount);
			Assert.AreEqual(1, myTarget.WriteCount);

			continuationHit = false;
			wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
			Assert.IsTrue(continuationHit);
			Assert.IsNull(lastException);
			Assert.AreEqual(2, myTarget.WriteCount);
			Assert.AreEqual(2, myTarget.FlushCount);
		}

		[Test]
		public void AutoFlushTargetWrapperAsyncTest1()
		{
			var myTarget = new MyAsyncTarget();
			var wrapper = new AutoFlushTargetWrapper(myTarget);
			myTarget.Initialize(null);
			wrapper.Initialize(null);
			var logEvent = new LogEventInfo();
			Exception lastException = null;
			var continuationHit = new ManualResetEvent(false);
			AsyncContinuation continuation =
				ex =>
				{
					lastException = ex;
					continuationHit.Set();
				};

			wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));

			continuationHit.WaitOne();
			Assert.IsNull(lastException);
			Assert.AreEqual(1, myTarget.FlushCount);
			Assert.AreEqual(1, myTarget.WriteCount);

			continuationHit.Reset();
			wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
			continuationHit.WaitOne();
			Assert.IsNull(lastException);
			Assert.AreEqual(2, myTarget.WriteCount);
			Assert.AreEqual(2, myTarget.FlushCount);
		}


		[Test]
		public void AutoFlushTargetWrapperAsyncWithExceptionTest1()
		{
			var myTarget = new MyAsyncTarget
			{
				ThrowExceptions = true,
			};

			var wrapper = new AutoFlushTargetWrapper(myTarget);
			myTarget.Initialize(null);
			wrapper.Initialize(null);
			var logEvent = new LogEventInfo();
			Exception lastException = null;
			var continuationHit = new ManualResetEvent(false);
			AsyncContinuation continuation =
				ex =>
				{
					lastException = ex;
					continuationHit.Set();
				};

			wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));

			continuationHit.WaitOne();
			Assert.IsNotNull(lastException);
			Assert.IsInstanceOfType(typeof(InvalidOperationException), lastException);

			// no flush on exception
			Assert.AreEqual(0, myTarget.FlushCount);
			Assert.AreEqual(1, myTarget.WriteCount);

			continuationHit.Reset();
			lastException = null;
			wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
			continuationHit.WaitOne();
			Assert.IsNotNull(lastException);
			Assert.IsInstanceOfType(typeof(InvalidOperationException), lastException);
			Assert.AreEqual(0, myTarget.FlushCount);
			Assert.AreEqual(2, myTarget.WriteCount);
		}

		class MyAsyncTarget : Target
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

		class MyTarget : Target
		{
			public int FlushCount { get; set; }
			public int WriteCount { get; set; }

			protected override void Write(LogEventInfo logEvent)
			{
				Assert.IsTrue(this.FlushCount <= this.WriteCount);
				this.WriteCount++;
			}

			protected override void FlushAsync(AsyncContinuation asyncContinuation)
			{
				this.FlushCount++;
				asyncContinuation(null);
			}
		}
	}
}
