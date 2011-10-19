
namespace NLog.UnitTests.Targets.Wrappers
{
	using System;
	using System.Collections.Generic;
	using NUnit.Framework;

#if !NUNIT
	using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
	using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
	using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
	using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
	using NLog.Internal;
	using NLog.Targets;
	using NLog.Targets.Wrappers;

	[TestFixture]
	public class RepeatingTargetWrapperTests : NLogTestBase
	{
		[Test]
		public void RepeatingTargetWrapperTest1()
		{
			var target = new MyTarget();
			var wrapper = new RepeatingTargetWrapper()
			{
				WrappedTarget = target,
				RepeatCount = 3,
			};
			wrapper.Initialize(null);
			target.Initialize(null);

			var exceptions = new List<Exception>();

			var events = new[]
			{
				new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger2", "Hello").WithContinuation(exceptions.Add),
			};

			wrapper.WriteAsyncLogEvents(events);

			// make sure all events went through and were replicated 3 times
			Assert.AreEqual(9, target.Events.Count);
			Assert.AreSame(events[0].LogEvent, target.Events[0]);
			Assert.AreSame(events[0].LogEvent, target.Events[1]);
			Assert.AreSame(events[0].LogEvent, target.Events[2]);
			Assert.AreSame(events[1].LogEvent, target.Events[3]);
			Assert.AreSame(events[1].LogEvent, target.Events[4]);
			Assert.AreSame(events[1].LogEvent, target.Events[5]);
			Assert.AreSame(events[2].LogEvent, target.Events[6]);
			Assert.AreSame(events[2].LogEvent, target.Events[7]);
			Assert.AreSame(events[2].LogEvent, target.Events[8]);

			Assert.AreEqual(events.Length, exceptions.Count, "Some continuations were not invoked.");
		}

		[Test]
		public void RepeatingTargetWrapperTest2()
		{
			var target = new MyTarget();
			target.ThrowExceptions = true;
			var wrapper = new RepeatingTargetWrapper()
			{
				WrappedTarget = target,
				RepeatCount = 3,
			};
			wrapper.Initialize(null);
			target.Initialize(null);

			var exceptions = new List<Exception>();

			var events = new []
			{
				new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger2", "Hello").WithContinuation(exceptions.Add),
			};

			wrapper.WriteAsyncLogEvents(events);

			// make sure all events went through but were registered only once
			// since repeating target wrapper will not repeat in case of exception.
			Assert.AreEqual(3, target.Events.Count);
			Assert.AreSame(events[0].LogEvent, target.Events[0]);
			Assert.AreSame(events[1].LogEvent, target.Events[1]);
			Assert.AreSame(events[2].LogEvent, target.Events[2]);

			Assert.AreEqual(events.Length, exceptions.Count, "Some continuations were not invoked.");
			foreach (var exception in exceptions)
			{
				Assert.IsNotNull(exception);
				Assert.AreEqual("Some exception has ocurred.", exception.Message);
			}
		}

		public class MyTarget : Target
		{
			public MyTarget()
			{
				this.Events = new List<LogEventInfo>();
			}

			public List<LogEventInfo> Events { get; set; }

			public bool ThrowExceptions { get; set; }

			protected override void Write(LogEventInfo logEvent)
			{
				this.Events.Add(logEvent);

				if (this.ThrowExceptions)
				{
					throw new InvalidOperationException("Some exception has ocurred.");
				}
			}
		}
	}
}
