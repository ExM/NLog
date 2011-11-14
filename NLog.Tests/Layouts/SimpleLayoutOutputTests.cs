using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using NLog.Common;
using NLog.Config;
using NLog.Internal;
using NLog.LayoutRenderers;
using NLog.Layouts;
using NLog.Common;

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
			l.DeepInitialize(CommonCfg);

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
			l.DeepInitialize(cfg);
			string output = l.Render(LogEventInfo.CreateNullEvent());
			Assert.AreEqual("xxyy", output);
		}

		[Test]
		public void SimpleLayoutCachingTest()
		{
			var l = new SimpleLayout("xx${level}yy");
			var ev = LogEventInfo.CreateNullEvent();
			l.DeepInitialize(CommonCfg);
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
						l.DeepInitialize(CommonCfg);
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
			lr.DeepInitialize(CommonCfg);
			lr.Render(LogEventInfo.CreateNullEvent());
			Assert.AreEqual(1, lr.InitCount);
			Assert.AreEqual(0, lr.CloseCount);

			lr.Close();
			Assert.AreEqual(1, lr.InitCount);
			Assert.AreEqual(1, lr.CloseCount);

			// second call to Close() will be ignored
			lr.Close();
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
			lr.Close();
			Assert.AreEqual(0, lr.InitCount);
			Assert.AreEqual(0, lr.CloseCount);

			lr.DeepInitialize(CommonCfg);
			Assert.AreEqual(1, lr.InitCount);

			// make sure render will not call another Init
			lr.Render(LogEventInfo.CreateNullEvent());
			Assert.AreEqual(1, lr.InitCount);
			Assert.AreEqual(0, lr.CloseCount);

			lr.Close();
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

		public class MockLayout : Layout, ISupportsInitialize
		{
			public int InitCount { get; set; }

			public int CloseCount { get; set; }

			protected override string GetFormattedMessage(LogEventInfo logEvent)
			{
				return "foo";
			}

			void ISupportsInitialize.Initialize(LoggingConfiguration configuration)
			{
				InitCount++;
			}

			void ISupportsInitialize.Close()
			{
				CloseCount++;
			}
		}
	}
}
