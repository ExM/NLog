using System;
using System.Text;
using NLog.Common;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Layouts;
using NUnit.Framework;

namespace NLog.UnitTests.Layouts
{
	[TestFixture]
	public class SimpleLayoutOutputTests : NLogTestBase
	{
		[Test]
		public void VeryLongRendererOutput()
		{
			int stringLength = 100000;

			SimpleLayout l = new string('x', stringLength) + "${message}";
			l.Initialize(CommonCfg);

			string output = l.Render(LogEventInfo.CreateNullEvent());
			Assert.AreEqual(new string('x', stringLength), output);
			string output2 = l.Render(LogEventInfo.CreateNullEvent());
			Assert.AreEqual(new string('x', stringLength), output);
			Assert.AreNotSame(output, output2);
		}

		[Test]
		public void LayoutRendererThrows()
		{
			ConfigurationItemFactory configurationItemFactory = new ConfigurationItemFactory();
			configurationItemFactory.LayoutRenderers.RegisterDefinition("throwsException", typeof(ThrowsExceptionRenderer));
			var cfg = new LoggingConfiguration(configurationItemFactory);
			SimpleLayout l = new SimpleLayout("xx${throwsException}yy", cfg);
			l.Initialize(cfg);
			string output = l.Render(LogEventInfo.CreateNullEvent());
			Assert.AreEqual("xxyy", output);
		}

		[Test]
		public void SimpleLayoutCachingTest()
		{
			var l = new SimpleLayout("xx${level}yy");
			var ev = LogEventInfo.CreateNullEvent();
			l.Initialize(CommonCfg);
			string output1 = l.Render(ev);
			string output2 = l.Render(ev);
			Assert.AreSame(output1, output2);
		}

		[Test]
		public void SimpleLayoutToStringTest()
		{
			var l = new SimpleLayout("xx${level}yy");
			Assert.AreEqual("'xx${level}yy'", l.ToString());

			var l2 = new SimpleLayout("someFakeText", new LayoutRenderer[0]);
			Assert.AreEqual("'someFakeText'", l2.ToString());
		}

		[Test]
		public void LayoutRendererThrows2()
		{
			string internalLogOutput = RunAndCaptureInternalLog(
				() => 
					{
						ConfigurationItemFactory configurationItemFactory = new ConfigurationItemFactory();
						configurationItemFactory.LayoutRenderers.RegisterDefinition("throwsException", typeof(ThrowsExceptionRenderer));

						SimpleLayout l = new SimpleLayout("xx${throwsException:msg1}yy${throwsException:msg2}zz", new LoggingConfiguration(configurationItemFactory));
						l.Initialize(CommonCfg);
						string output = l.Render(LogEventInfo.CreateNullEvent());
						Assert.AreEqual("xxyyzz", output);
					}, 
					LogLevel.Warn);

			Assert.IsTrue(internalLogOutput.IndexOf("msg1") >= 0, internalLogOutput);
			Assert.IsTrue(internalLogOutput.IndexOf("msg2") >= 0, internalLogOutput);
		}

		[Test]
		public void LayoutInitTest1()
		{
			var lr = new MockLayout();
			Assert.AreEqual(0, lr.InitCount);
			Assert.AreEqual(0, lr.CloseCount);

			// make sure render will call Init
			using(lr.Initialize(CommonCfg))
			{
				lr.Render(LogEventInfo.CreateNullEvent());
				Assert.AreEqual(1, lr.InitCount);
				Assert.AreEqual(0, lr.CloseCount);
			}
			Assert.AreEqual(1, lr.InitCount);
			Assert.AreEqual(1, lr.CloseCount);

			// second call to Close() will be ignored
			((ISupportsInitialize)lr).Close();
			Assert.AreEqual(1, lr.InitCount);
			Assert.AreEqual(1, lr.CloseCount);
		}

		[Test]
		public void LayoutInitTest2()
		{
			var lr = new MockLayout();
			Assert.AreEqual(0, lr.InitCount);
			Assert.AreEqual(0, lr.CloseCount);

			// calls to Close() will be ignored because 
			((ISupportsInitialize)lr).Close();
			Assert.AreEqual(0, lr.InitCount);
			Assert.AreEqual(0, lr.CloseCount);

			using(lr.Initialize(CommonCfg))
			{
				Assert.AreEqual(1, lr.InitCount);
	
				// make sure render will not call another Init
				lr.Render(LogEventInfo.CreateNullEvent());
				Assert.AreEqual(1, lr.InitCount);
				Assert.AreEqual(0, lr.CloseCount);
			}
			Assert.AreEqual(1, lr.InitCount);
			Assert.AreEqual(1, lr.CloseCount);
		}

		public class ThrowsExceptionRenderer : LayoutRenderer
		{
			public ThrowsExceptionRenderer()
			{
				this.Message = "Some message.";
			}

			[RequiredParameter]
			[DefaultParameter]
			public string Message { get; set; }

			protected override void Append(StringBuilder builder, LogEventInfo logEvent)
			{
				throw new InvalidOperationException(this.Message);
			}
		}

		public class MockLayout : Layout
		{
			public int InitCount { get; set; }

			public int CloseCount { get; set; }

			protected override string GetFormattedMessage(LogEventInfo logEvent)
			{
				return "foo";
			}

			protected override void InternalInit(LoggingConfiguration cfg)
			{
				base.InternalInit(cfg);
				InitCount++;
			}

			protected override void InternalClose()
			{
				base.InternalClose();
				CloseCount++;
			}
		}
	}
}
