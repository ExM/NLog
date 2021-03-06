using System;
using System.Threading;
using NUnit.Framework;
using NLog.Common;
using NLog.Internal;
using NLog.Targets.Wrappers;

namespace NLog.UnitTests.Targets.Wrappers
{
	[TestFixture]
	public class AsyncRequestQueueTests : NLogTestBase
	{
		[Test]
		public void AsyncRequestQueueWithDiscardBehaviorTest()
		{
			var ev1 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
			var ev2 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
			var ev3 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
			var ev4 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });

			var queue = new AsyncRequestQueue(3, AsyncTargetWrapperOverflowAction.Discard);
			Assert.AreEqual(3, queue.RequestLimit);
			Assert.AreEqual(AsyncTargetWrapperOverflowAction.Discard, queue.OnOverflow);
			Assert.AreEqual(0, queue.RequestCount);
			queue.Enqueue(ev1);
			Assert.AreEqual(1, queue.RequestCount);
			queue.Enqueue(ev2);
			Assert.AreEqual(2, queue.RequestCount);
			queue.Enqueue(ev3);
			Assert.AreEqual(3, queue.RequestCount);
			queue.Enqueue(ev4);
			Assert.AreEqual(3, queue.RequestCount);

			AsyncLogEventInfo[] logEventInfos = queue.DequeueBatch(10);
			Assert.AreEqual(0, queue.RequestCount);

			// ev1 is lost
			Assert.AreSame(logEventInfos[0].LogEvent, ev2.LogEvent);
			Assert.AreSame(logEventInfos[1].LogEvent, ev3.LogEvent);
			Assert.AreSame(logEventInfos[2].LogEvent, ev4.LogEvent);
			Assert.AreSame(logEventInfos[0].Continuation, ev2.Continuation);
			Assert.AreSame(logEventInfos[1].Continuation, ev3.Continuation);
			Assert.AreSame(logEventInfos[2].Continuation, ev4.Continuation);
		}

		[Test]
		public void AsyncRequestQueueWithGrowBehaviorTest()
		{
			var ev1 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
			var ev2 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
			var ev3 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
			var ev4 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
			
			var queue = new AsyncRequestQueue(3, AsyncTargetWrapperOverflowAction.Grow);
			Assert.AreEqual(3, queue.RequestLimit);
			Assert.AreEqual(AsyncTargetWrapperOverflowAction.Grow, queue.OnOverflow);
			Assert.AreEqual(0, queue.RequestCount);
			queue.Enqueue(ev1);
			Assert.AreEqual(1, queue.RequestCount);
			queue.Enqueue(ev2);
			Assert.AreEqual(2, queue.RequestCount);
			queue.Enqueue(ev3);
			Assert.AreEqual(3, queue.RequestCount);
			queue.Enqueue(ev4);
			Assert.AreEqual(4, queue.RequestCount);

			AsyncLogEventInfo[] logEventInfos = queue.DequeueBatch(10);
			int result = logEventInfos.Length;

			Assert.AreEqual(4, result);
			Assert.AreEqual(0, queue.RequestCount);

			// ev1 is lost
			Assert.AreSame(logEventInfos[0].LogEvent, ev1.LogEvent);
			Assert.AreSame(logEventInfos[1].LogEvent, ev2.LogEvent);
			Assert.AreSame(logEventInfos[2].LogEvent, ev3.LogEvent);
			Assert.AreSame(logEventInfos[3].LogEvent, ev4.LogEvent);
			Assert.AreSame(logEventInfos[0].Continuation, ev1.Continuation);
			Assert.AreSame(logEventInfos[1].Continuation, ev2.Continuation);
			Assert.AreSame(logEventInfos[2].Continuation, ev3.Continuation);
			Assert.AreSame(logEventInfos[3].Continuation, ev4.Continuation);
		}

		[Test]
		public void AsyncRequestQueueWithBlockBehavior()
		{
			var queue = new AsyncRequestQueue(10, AsyncTargetWrapperOverflowAction.Block);

			ManualResetEvent producerFinished = new ManualResetEvent(false);

			int pushingEvent = 0;

			ThreadPool.QueueUserWorkItem(
				s =>
				{
					// producer thread
					for (int i = 0; i < 1000; ++i)
					{
						AsyncLogEventInfo logEvent = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
						logEvent.LogEvent.Message = "msg" + i;
						
						// Console.WriteLine("Pushing event {0}", i);
						pushingEvent = i;
						queue.Enqueue(logEvent);
					}

					producerFinished.Set();
				});

			// consumer thread
			AsyncLogEventInfo[] logEventInfos;
			int total = 0;

			while (total < 500)
			{
				int left = 500 - total;

				logEventInfos = queue.DequeueBatch(left);
				int got = logEventInfos.Length;
				Assert.IsTrue(got <= queue.RequestLimit);
				total += got;
			}

			Thread.Sleep(500);

			// producer is blocked on trying to push event #510
			Assert.AreEqual(510, pushingEvent);
			queue.DequeueBatch(1);
			total++;
			Thread.Sleep(500);

			// producer is now blocked on trying to push event #511

			Assert.AreEqual(511, pushingEvent);
			while (total < 1000)
			{
				int left = 1000 - total;

				logEventInfos = queue.DequeueBatch(left);
				int got = logEventInfos.Length;
				Assert.IsTrue(got <= queue.RequestLimit);
				total += got;
			}

			// producer should now finish
			producerFinished.WaitOne();
		}

		[Test]
		public void AsyncRequestQueueClearTest()
		{
			var ev1 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
			var ev2 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
			var ev3 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
			var ev4 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });

			var queue = new AsyncRequestQueue(3, AsyncTargetWrapperOverflowAction.Grow);
			Assert.AreEqual(3, queue.RequestLimit);
			Assert.AreEqual(AsyncTargetWrapperOverflowAction.Grow, queue.OnOverflow);
			Assert.AreEqual(0, queue.RequestCount);
			queue.Enqueue(ev1);
			Assert.AreEqual(1, queue.RequestCount);
			queue.Enqueue(ev2);
			Assert.AreEqual(2, queue.RequestCount);
			queue.Enqueue(ev3);
			Assert.AreEqual(3, queue.RequestCount);
			queue.Enqueue(ev4);
			Assert.AreEqual(4, queue.RequestCount);
			queue.Clear();
			Assert.AreEqual(0, queue.RequestCount);

			AsyncLogEventInfo[] logEventInfos;

			logEventInfos = queue.DequeueBatch(10);
			int result = logEventInfos.Length;
			Assert.AreEqual(0, result);
			Assert.AreEqual(0, queue.RequestCount);
		}
	}
}
