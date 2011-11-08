using NLog.Config;

namespace NLog.UnitTests.Targets.Wrappers
{
	using System;
	using System.Threading;
	using NUnit.Framework;
	using NLog.Common;
	using NLog.Internal;
	using NLog.Targets;
	using NLog.Targets.Wrappers;
	using System.Collections.Generic;

	[TestFixture]
	public class AsyncTargetWrapperTests : NLogTestBase
	{
		[Test]
		public void AsyncTargetWrapperInitTest()
		{
			var myTarget = new MyTarget();
			var targetWrapper = new AsyncTargetWrapper(myTarget, 300, AsyncTargetWrapperOverflowAction.Grow);
			Assert.AreEqual(AsyncTargetWrapperOverflowAction.Grow, targetWrapper.OverflowAction);
			Assert.AreEqual(300, targetWrapper.QueueLimit);
			Assert.AreEqual(50, targetWrapper.TimeToSleepBetweenBatches);
			Assert.AreEqual(100, targetWrapper.BatchSize);
		}

		[Test]
		public void AsyncTargetWrapperInitTest2()
		{
			var myTarget = new MyTarget();
			var targetWrapper = new AsyncTargetWrapper()
			{
				WrappedTarget = myTarget,
			};

			Assert.AreEqual(AsyncTargetWrapperOverflowAction.Discard, targetWrapper.OverflowAction);
			Assert.AreEqual(10000, targetWrapper.QueueLimit);
			Assert.AreEqual(50, targetWrapper.TimeToSleepBetweenBatches);
			Assert.AreEqual(100, targetWrapper.BatchSize);
		}

		[Test]
		public void AsyncTargetWrapperSyncTest1()
		{
			var myTarget = new MyTarget();
			var targetWrapper = new AsyncTargetWrapper
			{
				WrappedTarget = myTarget,
			};
			targetWrapper.Initialize(null);
			myTarget.Initialize(null);

			var logEvent = new LogEventInfo();
			Exception lastException = null;
			ManualResetEvent continuationHit = new ManualResetEvent(false);
			Thread continuationThread = null;
			AsyncContinuation continuation =
				ex =>
					{
						lastException = ex;
						continuationThread = Thread.CurrentThread;
						continuationHit.Set();
					};

			targetWrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));

			// continuation was not hit 
			continuationHit.WaitOne();
			Assert.AreNotSame(continuationThread, Thread.CurrentThread);
			Assert.IsNull(lastException);
			Assert.AreEqual(1, myTarget.WriteCount);

			continuationHit.Reset();
			targetWrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
			continuationHit.WaitOne();
			Assert.AreNotSame(continuationThread, Thread.CurrentThread);
			Assert.IsNull(lastException);
			Assert.AreEqual(2, myTarget.WriteCount);
		}

		[Test]
		public void AsyncTargetWrapperAsyncTest1()
		{
			var myTarget = new MyAsyncTarget();
			var targetWrapper = new AsyncTargetWrapper(myTarget);
			targetWrapper.Initialize(null);
			myTarget.Initialize(null);
			var logEvent = new LogEventInfo();
			Exception lastException = null;
			var continuationHit = new ManualResetEvent(false);
			AsyncContinuation continuation =
				ex =>
				{
					lastException = ex;
					continuationHit.Set();
				};

			targetWrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));

			continuationHit.WaitOne();
			Assert.IsNull(lastException);
			Assert.AreEqual(1, myTarget.WriteCount);

			continuationHit.Reset();
			targetWrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
			continuationHit.WaitOne();
			Assert.IsNull(lastException);
			Assert.AreEqual(2, myTarget.WriteCount);
		}

		[Test]
		public void AsyncTargetWrapperAsyncWithExceptionTest1()
		{
			var myTarget = new MyAsyncTarget
			{
				ThrowExceptions = true,
			};

			var targetWrapper = new AsyncTargetWrapper(myTarget);
			targetWrapper.Initialize(null);
			myTarget.Initialize(null);
			var logEvent = new LogEventInfo();
			Exception lastException = null;
			var continuationHit = new ManualResetEvent(false);
			AsyncContinuation continuation =
				ex =>
				{
					lastException = ex;
					continuationHit.Set();
				};

			targetWrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));

			continuationHit.WaitOne();
			Assert.IsNotNull(lastException);
			Assert.IsInstanceOf<InvalidOperationException>(lastException);

			// no flush on exception
			Assert.AreEqual(0, myTarget.FlushCount);
			Assert.AreEqual(1, myTarget.WriteCount);

			continuationHit.Reset();
			lastException = null;
			targetWrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
			continuationHit.WaitOne();
			Assert.IsNotNull(lastException);
			Assert.IsInstanceOf<InvalidOperationException>(lastException);
			Assert.AreEqual(0, myTarget.FlushCount);
			Assert.AreEqual(2, myTarget.WriteCount);
		}

		[Test]
		public void AsyncTargetWrapperFlushTest()
		{
			var myTarget = new MyAsyncTarget
			{
				ThrowExceptions = true,
			   
			};

			var targetWrapper = new AsyncTargetWrapper(myTarget)
			{
				OverflowAction = AsyncTargetWrapperOverflowAction.Grow,
			};

			targetWrapper.Initialize(null);
			myTarget.Initialize(null);

			List<Exception> exceptions = new List<Exception>();

			int eventCount = 5000;

			for (int i = 0; i < eventCount; ++i)
			{
				targetWrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(
					ex =>
					{
						lock (exceptions)
						{
							exceptions.Add(ex);
						}
					}));
			}

			Exception lastException = null;
			ManualResetEvent mre = new ManualResetEvent(false);

			string internalLog = RunAndCaptureInternalLog(
				() =>
				{
					targetWrapper.Flush(
						cont =>
						{
							try
							{
								// by this time all continuations should be completed
								Assert.AreEqual(eventCount, exceptions.Count);

								// with just 1 flush of the target
								Assert.AreEqual(1, myTarget.FlushCount);

								// and all writes should be accounted for
								Assert.AreEqual(eventCount, myTarget.WriteCount);
							}
							catch (Exception ex)
							{
								lastException = ex;
							}
							finally
							{
								mre.Set();
							}
						});
					mre.WaitOne();
				},
				LogLevel.Trace);

			if (lastException != null)
			{
				Assert.Fail(lastException.ToString() + "\r\n" + internalLog);
			}
		}

		[Test]
		public void AsyncTargetWrapperCloseTest()
		{
			var myTarget = new MyAsyncTarget
			{
				ThrowExceptions = true,

			};

			var targetWrapper = new AsyncTargetWrapper(myTarget)
			{
				OverflowAction = AsyncTargetWrapperOverflowAction.Grow,
				TimeToSleepBetweenBatches = 1000,
			};

			targetWrapper.Initialize(null);
			myTarget.Initialize(null);

			bool continuationHit = false;

			targetWrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(ex => { continuationHit = true; }));

			// quickly close the target before the timer elapses
			targetWrapper.Close();

			// continuation will not be hit because the thread is down.
			Thread.Sleep(1000);
			Assert.IsFalse(continuationHit);
		}

		[Test]
		public void AsyncTargetWrapperExceptionTest()
		{
			var targetWrapper = new AsyncTargetWrapper
			{
				OverflowAction = AsyncTargetWrapperOverflowAction.Grow,
				TimeToSleepBetweenBatches = 500,
				WrappedTarget = new DebugTarget(),
			};

			targetWrapper.Initialize(new LoggingConfiguration());

			// null out wrapped target - will cause exception on the timer thread
			targetWrapper.WrappedTarget = null;

			string internalLog = RunAndCaptureInternalLog(
				() =>
				{
					targetWrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(ex => { }));
					Thread.Sleep(3000);
				},
				LogLevel.Trace);

			targetWrapper.Close();
			Assert.IsTrue(internalLog.StartsWith("Error Error in lazy writer timer procedure: System.NullReferenceException", StringComparison.Ordinal), internalLog);
		}

		class MyAsyncTarget : Target
		{
			public int FlushCount;
			public int WriteCount;

			protected override void Write(LogEventInfo logEvent)
			{
				throw new NotSupportedException();
			}

			protected override void Write(AsyncLogEventInfo logEvent)
			{
				Assert.IsTrue(this.FlushCount <= this.WriteCount);
				Interlocked.Increment(ref this.WriteCount);
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
				Interlocked.Increment(ref this.FlushCount);
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
