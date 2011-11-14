
namespace NLog.UnitTests.Targets.Wrappers
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using NUnit.Framework;
	using NLog.Common;
	using NLog.Internal;
	using NLog.Targets;
	using NLog.Targets.Wrappers;

	[TestFixture]
	public class FallbackGroupTargetTests : NLogTestBase
	{
		[Test]
		public void FallbackGroupTargetSyncTest1()
		{
			var myTarget1 = new MyTarget();
			var myTarget2 = new MyTarget();
			var myTarget3 = new MyTarget();

			var wrapper = new FallbackGroupTarget()
			{
				Targets = { myTarget1, myTarget2, myTarget3 },
			};

			wrapper.DeepInitialize(CommonCfg);

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

			Assert.AreEqual(10, myTarget1.WriteCount);
			Assert.AreEqual(0, myTarget2.WriteCount);
			Assert.AreEqual(0, myTarget3.WriteCount);

			Exception flushException = null;
			var flushHit = new ManualResetEvent(false);
			wrapper.Flush(ex => { flushException = ex; flushHit.Set(); });

			flushHit.WaitOne();
			if (flushException != null)
			{
				Assert.Fail(flushException.ToString());
			}
		}

		[Test]
		public void FallbackGroupTargetSyncTest2()
		{
			// fail once
			var myTarget1 = new MyTarget() { FailCounter = 1 };
			var myTarget2 = new MyTarget();
			var myTarget3 = new MyTarget();

			var wrapper = new FallbackGroupTarget()
			{
				Targets = { myTarget1, myTarget2, myTarget3 },
			};

			wrapper.DeepInitialize(CommonCfg);

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

			Assert.AreEqual(1, myTarget1.WriteCount);
			Assert.AreEqual(10, myTarget2.WriteCount);
			Assert.AreEqual(0, myTarget3.WriteCount);

			Exception flushException = null;
			var flushHit = new ManualResetEvent(false);
			wrapper.Flush(ex => { flushException = ex; flushHit.Set(); });

			flushHit.WaitOne();
			if (flushException != null)
			{
				Assert.Fail(flushException.ToString());
			}
		}

		[Test]
		public void FallbackGroupTargetSyncTest3()
		{
			// fail once
			var myTarget1 = new MyTarget() { FailCounter = 1 };
			var myTarget2 = new MyTarget() { FailCounter = 1 };
			var myTarget3 = new MyTarget();

			var wrapper = new FallbackGroupTarget()
			{
				Targets = { myTarget1, myTarget2, myTarget3 },
			};

			wrapper.DeepInitialize(CommonCfg);

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

			Assert.AreEqual(1, myTarget1.WriteCount);
			Assert.AreEqual(1, myTarget2.WriteCount);
			Assert.AreEqual(10, myTarget3.WriteCount);

			Exception flushException = null;
			var flushHit = new ManualResetEvent(false);
			wrapper.Flush(ex => { flushException = ex; flushHit.Set(); });

			flushHit.WaitOne();
			if (flushException != null)
			{
				Assert.Fail(flushException.ToString());
			}
		}

		[Test]
		public void FallbackGroupTargetSyncTest4()
		{
			// fail once
			var myTarget1 = new MyTarget() { FailCounter = 1 };
			var myTarget2 = new MyTarget();
			var myTarget3 = new MyTarget();

			var wrapper = new FallbackGroupTarget()
			{
				Targets = { myTarget1, myTarget2, myTarget3 },
				ReturnToFirstOnSuccess = true,
			};

			wrapper.DeepInitialize(CommonCfg);

			List<Exception> exceptions = new List<Exception>();

			// no exceptions
			for (int i = 0; i < 10; ++i)
			{
				wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
			}

			// sequence is like this:
			// t1(fail), t2(success), t1(success), ... t1(success)
			Assert.AreEqual(10, exceptions.Count);
			foreach (var e in exceptions)
			{
				Assert.IsNull(e);
			}

			Assert.AreEqual(10, myTarget1.WriteCount);
			Assert.AreEqual(1, myTarget2.WriteCount);
			Assert.AreEqual(0, myTarget3.WriteCount);

			Exception flushException = null;
			var flushHit = new ManualResetEvent(false);
			wrapper.Flush(ex => { flushException = ex; flushHit.Set(); });

			flushHit.WaitOne();
			if (flushException != null)
			{
				Assert.Fail(flushException.ToString());
			}
		}

		[Test]
		public void FallbackGroupTargetSyncTest5()
		{
			// fail once
			var myTarget1 = new MyTarget() { FailCounter = 3 };
			var myTarget2 = new MyTarget() { FailCounter = 3 };
			var myTarget3 = new MyTarget() { FailCounter = 3 };

			var wrapper = new FallbackGroupTarget()
			{
				Targets = { myTarget1, myTarget2, myTarget3 },
				ReturnToFirstOnSuccess = true,
			};

			wrapper.DeepInitialize(CommonCfg);

			List<Exception> exceptions = new List<Exception>();

			// no exceptions
			for (int i = 0; i < 10; ++i)
			{
				wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
			}

			Assert.AreEqual(10, exceptions.Count);
			for (int i = 0; i < 10; ++i)
			{
				if (i < 3)
				{
					Assert.IsNotNull(exceptions[i]);
				}
				else
				{
					Assert.IsNull(exceptions[i]);
				}
			}

			Assert.AreEqual(10, myTarget1.WriteCount);
			Assert.AreEqual(3, myTarget2.WriteCount);
			Assert.AreEqual(3, myTarget3.WriteCount);

			Exception flushException = null;
			var flushHit = new ManualResetEvent(false);
			wrapper.Flush(ex => { flushException = ex; flushHit.Set(); });

			flushHit.WaitOne();
			if (flushException != null)
			{
				Assert.Fail(flushException.ToString());
			}
		}

		[Test]
		public void FallbackGroupTargetSyncTest6()
		{
			// fail once
			var myTarget1 = new MyTarget() { FailCounter = 10 };
			var myTarget2 = new MyTarget() { FailCounter = 3 };
			var myTarget3 = new MyTarget() { FailCounter = 3 };

			var wrapper = new FallbackGroupTarget()
			{
				Targets = { myTarget1, myTarget2, myTarget3 },
				ReturnToFirstOnSuccess = true,
			};

			wrapper.DeepInitialize(CommonCfg);

			List<Exception> exceptions = new List<Exception>();

			// no exceptions
			for (int i = 0; i < 10; ++i)
			{
				wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
			}

			Assert.AreEqual(10, exceptions.Count);
			for (int i = 0; i < 10; ++i)
			{
				if (i < 3)
				{
					// for the first 3 rounds, no target is available
					Assert.IsNotNull(exceptions[i]);
					Assert.IsInstanceOf<InvalidOperationException>(exceptions[i]);
					Assert.AreEqual("Some failure.", exceptions[i].Message);
				}
				else
				{
					Assert.IsNull(exceptions[i], Convert.ToString(exceptions[i]));
				}
			}

			Assert.AreEqual(10, myTarget1.WriteCount);
			Assert.AreEqual(10, myTarget2.WriteCount);
			Assert.AreEqual(3, myTarget3.WriteCount);

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

		class MyTarget : Target
		{
			public int FlushCount { get; set; }
			public int WriteCount { get; set; }
			public int FailCounter { get; set; }

			protected override void Write(LogEventInfo logEvent)
			{
				Assert.IsTrue(this.FlushCount <= this.WriteCount);
				this.WriteCount++;

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
