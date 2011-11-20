using System;
using NUnit.Framework;
using NLog.Common;
using NLog.Internal;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace NLog.UnitTests.Targets.Wrappers
{
	[TestFixture]
	public class WrapperTargetBaseTests : NLogTestBase
	{
		[Test]
		public void WrapperTargetToStringTest()
		{
			var wrapper = new MyWrapper
			{
				WrappedTarget = new DebugTarget() { Name = "foo" },
			};

			var wrapper2 = new MyWrapper()
			{
				WrappedTarget = wrapper,
			};

			Assert.AreEqual("MyWrapper(MyWrapper(Debug Target[foo]))", wrapper2.ToString());
		}

		[Test]
		public void WrapperTargetFlushTest()
		{
			var wrapped = new MyWrappedTarget();

			var wrapper = new MyWrapper
			{
				WrappedTarget = wrapped,
			};

			wrapper.Initialize(CommonCfg);

			wrapper.Flush(ex => { });
			Assert.AreEqual(1, wrapped.FlushCount);
		}

		[Test]
		public void WrapperTargetDefaultWriteTest()
		{
			Exception lastException = null;
			var wrapper = new MyWrapper();
			wrapper.WrappedTarget = new MyWrappedTarget();
			wrapper.Initialize(CommonCfg);
			wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(ex => lastException = ex));
			Assert.IsNotNull(lastException);
			Assert.IsInstanceOf<NotSupportedException>(lastException);
		}

		public class MyWrapper : WrapperTargetBase
		{
		}

		public class MyWrappedTarget : Target
		{
			public int FlushCount { get; set; }

			protected override void FlushAsync(Action<Exception> asyncContinuation)
			{
				this.FlushCount++;
				base.FlushAsync(asyncContinuation);
			}

			protected override void Write(LogEventInfo logEvent)
			{
				throw new NotImplementedException();
			}
		}
	}
}
