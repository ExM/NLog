using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using NLog.Common;
using NLog.Internal;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace NLog.UnitTests.Targets.Wrappers
{
	[TestFixture]
	public class RandomizeGroupTargetTests : NLogTestBase
	{
		[Test]
		public void RandomizeGroupSyncTest1()
		{
			var myTarget1 = new MyTarget();
			var myTarget2 = new MyTarget();
			var myTarget3 = new MyTarget();

			var wrapper = new RandomizeGroupTarget()
			{
				Targets = { myTarget1, myTarget2, myTarget3 },
			};

			wrapper.Initialize(CommonCfg);

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

			Assert.AreEqual(10, myTarget1.WriteCount + myTarget2.WriteCount + myTarget3.WriteCount);

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
		public void RandomizeGroupSyncTest2()
		{
			var wrapper = new RandomizeGroupTarget()
			{
				// no targets
			};

			wrapper.Initialize(CommonCfg);

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

			protected override void FlushAsync(Action<Exception> asyncContinuation)
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

			protected override void FlushAsync(Action<Exception> asyncContinuation)
			{
				this.FlushCount++;
				asyncContinuation(null);
			}
		}
	}
}
