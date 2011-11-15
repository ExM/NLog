
namespace NLog.UnitTests.Targets.Wrappers
{
	using System;
	using System.Threading;
	using NUnit.Framework;
	using NLog.Common;
	using NLog.Conditions;
	using NLog.Internal;
	using NLog.Targets;
	using NLog.Targets.Wrappers;

	[TestFixture]
	public class FilteringTargetWrapperTests : NLogTestBase
	{
		[Test]
		public void FilteringTargetWrapperSyncTest1()
		{
			var myMockCondition = new MyMockCondition(true);
			var myTarget = new MyTarget();
			var wrapper = new FilteringTargetWrapper
			{
				WrappedTarget = myTarget,
				Condition = myMockCondition,
			};

			wrapper.Initialize(CommonCfg);
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

			Assert.AreEqual(1, myMockCondition.CallCount);

			Assert.IsTrue(continuationHit);
			Assert.IsNull(lastException);
			Assert.AreEqual(1, myTarget.WriteCount);

			continuationHit = false;
			wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
			Assert.IsTrue(continuationHit);
			Assert.IsNull(lastException);
			Assert.AreEqual(2, myTarget.WriteCount);
			Assert.AreEqual(2, myMockCondition.CallCount);
		}

		[Test]
		public void FilteringTargetWrapperAsyncTest1()
		{
			var myMockCondition = new MyMockCondition(true);
			var myTarget = new MyAsyncTarget();
			var wrapper = new FilteringTargetWrapper(myTarget, myMockCondition);

			wrapper.Initialize(CommonCfg);
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
			Assert.AreEqual(1, myTarget.WriteCount);
			Assert.AreEqual(1, myMockCondition.CallCount);

			continuationHit.Reset();
			wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
			continuationHit.WaitOne();
			Assert.IsNull(lastException);
			Assert.AreEqual(2, myTarget.WriteCount);
			Assert.AreEqual(2, myMockCondition.CallCount);
		}

		[Test]
		public void FilteringTargetWrapperAsyncWithExceptionTest1()
		{
			var myMockCondition = new MyMockCondition(true);
			var myTarget = new MyAsyncTarget
			{
				ThrowExceptions = true,
			};

			var wrapper = new FilteringTargetWrapper(myTarget, myMockCondition);
			wrapper.Initialize(CommonCfg);
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
			Assert.IsInstanceOf<InvalidOperationException>(lastException);

			Assert.AreEqual(1, myTarget.WriteCount);
			Assert.AreEqual(1, myMockCondition.CallCount);

			continuationHit.Reset();
			lastException = null;
			wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
			continuationHit.WaitOne();
			Assert.IsNotNull(lastException);
			Assert.IsInstanceOf<InvalidOperationException>(lastException);
			Assert.AreEqual(2, myTarget.WriteCount);
			Assert.AreEqual(2, myMockCondition.CallCount);
		}

		[Test]
		public void FilteringTargetWrapperSyncTest2()
		{
			var myMockCondition = new MyMockCondition(false);
			var myTarget = new MyTarget();
			var wrapper = new FilteringTargetWrapper
			{
				WrappedTarget = myTarget,
				Condition = myMockCondition,
			};

			wrapper.Initialize(CommonCfg);
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

			Assert.AreEqual(1, myMockCondition.CallCount);

			Assert.IsTrue(continuationHit);
			Assert.IsNull(lastException);
			Assert.AreEqual(0, myTarget.WriteCount);
			Assert.AreEqual(1, myMockCondition.CallCount);

			continuationHit = false;
			wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
			Assert.IsTrue(continuationHit);
			Assert.IsNull(lastException);
			Assert.AreEqual(0, myTarget.WriteCount);
			Assert.AreEqual(2, myMockCondition.CallCount);
		}

		[Test]
		public void FilteringTargetWrapperAsyncTest2()
		{
			var myMockCondition = new MyMockCondition(false);
			var myTarget = new MyAsyncTarget();
			var wrapper = new FilteringTargetWrapper(myTarget, myMockCondition);
			wrapper.Initialize(CommonCfg);
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
			Assert.AreEqual(0, myTarget.WriteCount);
			Assert.AreEqual(1, myMockCondition.CallCount);

			continuationHit.Reset();
			wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
			continuationHit.WaitOne();
			Assert.IsNull(lastException);
			Assert.AreEqual(0, myTarget.WriteCount);
			Assert.AreEqual(2, myMockCondition.CallCount);
		}

		[Test]
		public void FilteringTargetWrapperAsyncWithExceptionTest2()
		{
			var myMockCondition = new MyMockCondition(false);
			var myTarget = new MyAsyncTarget
			{
				ThrowExceptions = true,
			};
			var wrapper = new FilteringTargetWrapper(myTarget, myMockCondition);
			wrapper.Initialize(CommonCfg);
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

			Assert.AreEqual(0, myTarget.WriteCount);
			Assert.AreEqual(1, myMockCondition.CallCount);

			continuationHit.Reset();
			lastException = null;
			wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
			continuationHit.WaitOne();
			Assert.IsNull(lastException);
			Assert.AreEqual(0, myTarget.WriteCount);
			Assert.AreEqual(2, myMockCondition.CallCount);
		}

		class MyAsyncTarget : Target
		{
			public int WriteCount { get; private set; }

			protected override void Write(LogEventInfo logEvent)
			{
				throw new NotSupportedException();
			}

			protected override void Write(AsyncLogEventInfo logEvent)
			{
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

			public bool ThrowExceptions { get; set; }
		}

		class MyTarget : Target
		{
			public int WriteCount { get; set; }

			protected override void Write(LogEventInfo logEvent)
			{
				this.WriteCount++;
			}
		}

		class MyMockCondition : ConditionExpression
		{
			private bool result;

			public int CallCount { get; set; }

			public MyMockCondition(bool result)
			{
				this.result = result;
			}

			protected override object EvaluateNode(LogEventInfo context)
			{
				this.CallCount++;
				return this.result;
			}

			public override string ToString()
			{
				return "fake";
			}
		}
	}
}
