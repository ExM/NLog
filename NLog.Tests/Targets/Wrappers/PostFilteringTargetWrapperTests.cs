
namespace NLog.UnitTests.Targets.Wrappers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using NUnit.Framework;
	using NLog.Common;
	using NLog.Internal;
	using NLog.Targets;
	using NLog.Targets.Wrappers;

	[TestFixture]
	public class PostFilteringTargetWrapperTests : NLogTestBase
	{
		[Test]
		public void PostFilteringTargetWrapperUsingDefaultFilterTest()
		{
			var target = new MyTarget();
			var wrapper = new PostFilteringTargetWrapper()
			{
				WrappedTarget = target,
				Rules =
				{
					// if we had any warnings, log debug too
					new FilteringRule("level >= LogLevel.Warn", "level >= LogLevel.Debug"),

					// when there is an error, emit everything
					new FilteringRule
					{
						Exists = "level >= LogLevel.Error", 
						Filter = "true",
					},
				},

				// by default log info and above
				DefaultFilter = "level >= LogLevel.Info",
			};

			wrapper.Initialize(CommonCfg);

			var exceptions = new List<Exception>();
			
			var events = new []
			{
				new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger2", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Trace, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger3", "Hello").WithContinuation(exceptions.Add),
			};

			wrapper.WriteAsyncLogEvents(events);

			// make sure all Info events went through
			Assert.AreEqual(3, target.Events.Count);
			Assert.AreSame(events[1].LogEvent, target.Events[0]);
			Assert.AreSame(events[2].LogEvent, target.Events[1]);
			Assert.AreSame(events[5].LogEvent, target.Events[2]);

			Assert.AreEqual(events.Length, exceptions.Count, "Some continuations were not invoked.");
		}

		[Test]
		public void PostFilteringTargetWrapperUsingDefaultNonFilterTest()
		{
			var target = new MyTarget();
			var wrapper = new PostFilteringTargetWrapper()
			{
				WrappedTarget = target,
				Rules =
				{
					// if we had any warnings, log debug too
					new FilteringRule("level >= LogLevel.Warn", "level >= LogLevel.Debug"),

					// when there is an error, emit everything
					new FilteringRule("level >= LogLevel.Error", "true"),
				},

				// by default log info and above
				DefaultFilter = "level >= LogLevel.Info",
			};

			wrapper.Initialize(CommonCfg);

			var exceptions = new List<Exception>();

			var events = new[]
			{
				new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger2", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Trace, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger3", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Warn, "Logger1", "Hello").WithContinuation(exceptions.Add),
			};

			string internalLogOutput = RunAndCaptureInternalLog(() => wrapper.WriteAsyncLogEvents(events), LogLevel.Trace);
			string expectedLogOutput = @"Trace Running PostFilteringWrapper Target[(unnamed)](MyTarget) on 7 events
Trace Rule matched: (level >= Warn)
Trace Filter to apply: (level >= Debug)
Trace After filtering: 6 events.
Trace Sending to MyTarget
";
			Assert.AreEqual(expectedLogOutput, internalLogOutput);

			// make sure all Debug,Info,Warn events went through
			Assert.AreEqual(6, target.Events.Count);
			Assert.AreSame(events[0].LogEvent, target.Events[0]);
			Assert.AreSame(events[1].LogEvent, target.Events[1]);
			Assert.AreSame(events[2].LogEvent, target.Events[2]);
			Assert.AreSame(events[3].LogEvent, target.Events[3]);
			Assert.AreSame(events[5].LogEvent, target.Events[4]);
			Assert.AreSame(events[6].LogEvent, target.Events[5]);

			Assert.AreEqual(events.Length, exceptions.Count, "Some continuations were not invoked.");
		}

		[Test]
		public void PostFilteringTargetWrapperUsingDefaultNonFilterTest2()
		{
			// in this case both rules would match, but first one is picked
			var target = new MyTarget();
			var wrapper = new PostFilteringTargetWrapper()
			{
				WrappedTarget = target,
				Rules =
				{
					// when there is an error, emit everything
					new FilteringRule("level >= LogLevel.Error", "true"),

					// if we had any warnings, log debug too
					new FilteringRule("level >= LogLevel.Warn", "level >= LogLevel.Debug"),
				},

				// by default log info and above
				DefaultFilter = "level >= LogLevel.Info",
			};

			wrapper.Initialize(CommonCfg);

			var exceptions = new List<Exception>();

			var events = new []
			{
				new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger2", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Trace, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger3", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Error, "Logger1", "Hello").WithContinuation(exceptions.Add),
			};

			var internalLogOutput = RunAndCaptureInternalLog(() => wrapper.WriteAsyncLogEvents(events), LogLevel.Trace);
			string expectedLogOutput = @"Trace Running PostFilteringWrapper Target[(unnamed)](MyTarget) on 7 events
Trace Rule matched: (level >= Error)
Trace Filter to apply: True
Trace After filtering: 7 events.
Trace Sending to MyTarget
";

			Assert.AreEqual(expectedLogOutput, internalLogOutput);

			// make sure all events went through
			Assert.AreEqual(7, target.Events.Count);
			Assert.AreSame(events[0].LogEvent, target.Events[0]);
			Assert.AreSame(events[1].LogEvent, target.Events[1]);
			Assert.AreSame(events[2].LogEvent, target.Events[2]);
			Assert.AreSame(events[3].LogEvent, target.Events[3]);
			Assert.AreSame(events[4].LogEvent, target.Events[4]);
			Assert.AreSame(events[5].LogEvent, target.Events[5]);
			Assert.AreSame(events[6].LogEvent, target.Events[6]);

			Assert.AreEqual(events.Length, exceptions.Count, "Some continuations were not invoked.");
		}

		[Test]
		public void PostFilteringTargetWrapperNoFiltersDefined()
		{
			var target = new MyTarget();
			var wrapper = new PostFilteringTargetWrapper()
			{
				WrappedTarget = target,
			};

			wrapper.Initialize(CommonCfg);

			var exceptions = new List<Exception>();

			var events = new[]
			{
				new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger2", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Trace, "Logger1", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Info, "Logger3", "Hello").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Error, "Logger1", "Hello").WithContinuation(exceptions.Add),
			};

			wrapper.WriteAsyncLogEvents(events);

			// make sure all events went through
			Assert.AreEqual(7, target.Events.Count);
			Assert.AreSame(events[0].LogEvent, target.Events[0]);
			Assert.AreSame(events[1].LogEvent, target.Events[1]);
			Assert.AreSame(events[2].LogEvent, target.Events[2]);
			Assert.AreSame(events[3].LogEvent, target.Events[3]);
			Assert.AreSame(events[4].LogEvent, target.Events[4]);
			Assert.AreSame(events[5].LogEvent, target.Events[5]);
			Assert.AreSame(events[6].LogEvent, target.Events[6]);

			Assert.AreEqual(events.Length, exceptions.Count, "Some continuations were not invoked.");
		}

		public class MyTarget : Target
		{
			public MyTarget()
			{
				this.Events = new List<LogEventInfo>();
			}

			public List<LogEventInfo> Events { get; set; }

			protected override void Write(LogEventInfo logEvent)
			{
				this.Events.Add(logEvent);
			}
		}
	}
}
