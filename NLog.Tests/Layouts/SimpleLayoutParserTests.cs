using NLog.Config;
using NLog.Common;
using System;
using NUnit.Framework;
using NLog.LayoutRenderers;
using NLog.LayoutRenderers.Wrappers;
using NLog.Layouts;

namespace NLog.UnitTests.Layouts
{
	[TestFixture]
	public class SimpleLayoutParserTests : NLogTestBase
	{
		[Test]
		public void SimpleTest()
		{
			SimpleLayout l = "${message}";
			l.Initialize(CommonCfg);
			Assert.AreEqual(1, l.Renderers.Count);
			Assert.IsInstanceOf<MessageLayoutRenderer>(l.Renderers[0]);
		}

		[Test]
		public void UnclosedTest()
		{
			new SimpleLayout("${message");
		}

		[Test]
		public void SingleParamTest()
		{
			SimpleLayout l = "${mdc:item=AAA}";
			l.Initialize(CommonCfg);
			Assert.AreEqual(1, l.Renderers.Count);
			MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
			Assert.IsNotNull(mdc);
			Assert.AreEqual("AAA", mdc.Item);
		}

		[Test]
		public void ValueWithColonTest()
		{
			SimpleLayout l = "${mdc:item=AAA\\:}";
			l.Initialize(CommonCfg);
			Assert.AreEqual(1, l.Renderers.Count);
			MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
			Assert.IsNotNull(mdc);
			Assert.AreEqual("AAA:", mdc.Item);
		}

		[Test]
		public void ValueWithBracketTest()
		{
			SimpleLayout l = "${mdc:item=AAA\\}\\:}";
			l.Initialize(CommonCfg);
			Assert.AreEqual("${mdc:item=AAA\\}\\:}", l.Text);
			Assert.AreEqual(1, l.Renderers.Count);
			MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
			Assert.IsNotNull(mdc);
			Assert.AreEqual("AAA}:", mdc.Item);
		}

		[Test]
		public void DefaultValueTest()
		{
			SimpleLayout l = "${mdc:BBB}";
			l.Initialize(CommonCfg);
			Assert.AreEqual(1, l.Renderers.Count);
			MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
			Assert.IsNotNull(mdc);
			Assert.AreEqual("BBB", mdc.Item);
		}

		[Test]
		public void DefaultValueWithBracketTest()
		{
			SimpleLayout l = "${mdc:AAA\\}\\:}";
			l.Initialize(CommonCfg);
			Assert.AreEqual(l.Text, "${mdc:AAA\\}\\:}");
			Assert.AreEqual(1, l.Renderers.Count);
			MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
			Assert.IsNotNull(mdc);
			Assert.AreEqual("AAA}:", mdc.Item);
		}

		[Test]
		public void DefaultValueWithOtherParametersTest()
		{
			SimpleLayout l = "${exception:message,type:separator=x}";
			l.Initialize(CommonCfg);
			Assert.AreEqual(1, l.Renderers.Count);
			ExceptionLayoutRenderer elr = l.Renderers[0] as ExceptionLayoutRenderer;
			Assert.IsNotNull(elr);
			Assert.AreEqual("message,type", elr.Format);
			Assert.AreEqual("x", elr.Separator);
		}

		[Test]
		public void EmptyValueTest()
		{
			SimpleLayout l = "${mdc:item=}";
			l.Initialize(CommonCfg);
			Assert.AreEqual(1, l.Renderers.Count);
			MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
			Assert.IsNotNull(mdc);
			Assert.AreEqual("", mdc.Item);
		}

		[Test]
		public void NestedLayoutTest()
		{
			SimpleLayout l = "${rot13:inner=${ndc:topFrames=3:separator=x}}";
			l.Initialize(CommonCfg);
			Assert.AreEqual(1, l.Renderers.Count);
			var lr = l.Renderers[0] as Rot13LayoutRendererWrapper;
			Assert.IsNotNull(lr);
			var nestedLayout = lr.Inner as SimpleLayout;
			Assert.IsNotNull(nestedLayout);
			Assert.AreEqual("${ndc:topFrames=3:separator=x}", nestedLayout.Text);
			Assert.AreEqual(1, nestedLayout.Renderers.Count);
			var ndcLayoutRenderer = nestedLayout.Renderers[0] as NdcLayoutRenderer;
			Assert.IsNotNull(ndcLayoutRenderer);
			Assert.AreEqual(3, ndcLayoutRenderer.TopFrames);
			Assert.AreEqual("x", ndcLayoutRenderer.Separator);
		}

		[Test]
		public void DoubleNestedLayoutTest()
		{
			SimpleLayout l = "${rot13:inner=${rot13:inner=${ndc:topFrames=3:separator=x}}}";
			l.Initialize(CommonCfg);
			Assert.AreEqual(1, l.Renderers.Count);
			var lr = l.Renderers[0] as Rot13LayoutRendererWrapper;
			Assert.IsNotNull(lr);
			var nestedLayout0 = lr.Inner as SimpleLayout;
			Assert.IsNotNull(nestedLayout0);
			Assert.AreEqual("${rot13:inner=${ndc:topFrames=3:separator=x}}", nestedLayout0.Text);
			var innerRot13 = nestedLayout0.Renderers[0] as Rot13LayoutRendererWrapper;
			var nestedLayout = innerRot13.Inner as SimpleLayout;
			Assert.IsNotNull(nestedLayout);
			Assert.AreEqual("${ndc:topFrames=3:separator=x}", nestedLayout.Text);
			Assert.AreEqual(1, nestedLayout.Renderers.Count);
			var ndcLayoutRenderer = nestedLayout.Renderers[0] as NdcLayoutRenderer;
			Assert.IsNotNull(ndcLayoutRenderer);
			Assert.AreEqual(3, ndcLayoutRenderer.TopFrames);
			Assert.AreEqual("x", ndcLayoutRenderer.Separator);
		}

		[Test]
		public void DoubleNestedLayoutWithDefaultLayoutParametersTest()
		{
			SimpleLayout l = "${rot13:${rot13:${ndc:topFrames=3:separator=x}}}";
			l.Initialize(CommonCfg);
			Assert.AreEqual(1, l.Renderers.Count);
			var lr = l.Renderers[0] as Rot13LayoutRendererWrapper;
			Assert.IsNotNull(lr);
			var nestedLayout0 = lr.Inner as SimpleLayout;
			Assert.IsNotNull(nestedLayout0);
			Assert.AreEqual("${rot13:${ndc:topFrames=3:separator=x}}", nestedLayout0.Text);
			var innerRot13 = nestedLayout0.Renderers[0] as Rot13LayoutRendererWrapper;
			var nestedLayout = innerRot13.Inner as SimpleLayout;
			Assert.IsNotNull(nestedLayout);
			Assert.AreEqual("${ndc:topFrames=3:separator=x}", nestedLayout.Text);
			Assert.AreEqual(1, nestedLayout.Renderers.Count);
			var ndcLayoutRenderer = nestedLayout.Renderers[0] as NdcLayoutRenderer;
			Assert.IsNotNull(ndcLayoutRenderer);
			Assert.AreEqual(3, ndcLayoutRenderer.TopFrames);
			Assert.AreEqual("x", ndcLayoutRenderer.Separator);
		}

		[Test]
		public void AmbientPropertyTest()
		{
			SimpleLayout l = "${message:padding=10}";
			l.Initialize(CommonCfg);
			Assert.AreEqual(1, l.Renderers.Count);
			var pad = l.Renderers[0] as PaddingLayoutRendererWrapper;
			Assert.IsNotNull(pad);
			var message = ((SimpleLayout)pad.Inner).Renderers[0] as MessageLayoutRenderer;
			Assert.IsNotNull(message);
		}

		[Test]
		[ExpectedException(typeof(NLogConfigurationException))]
		public void MissingLayoutRendererTest()
		{
			SimpleLayout l = "${rot13:${foobar}}";
			l.Initialize(CommonCfg);
		}

		[Test]
		public void DoubleAmbientPropertyTest()
		{
			SimpleLayout l = "${message:uppercase=true:padding=10}";
			l.Initialize(CommonCfg);
			Assert.AreEqual(1, l.Renderers.Count);
			var upperCase = l.Renderers[0] as UppercaseLayoutRendererWrapper;
			Assert.IsNotNull(upperCase);
			var pad = ((SimpleLayout)upperCase.Inner).Renderers[0] as PaddingLayoutRendererWrapper;
			Assert.IsNotNull(pad);
			var message = ((SimpleLayout)pad.Inner).Renderers[0] as MessageLayoutRenderer;
			Assert.IsNotNull(message);
		}

		[Test]
		public void ReverseDoubleAmbientPropertyTest()
		{
			SimpleLayout l = "${message:padding=10:uppercase=true}";
			l.Initialize(CommonCfg);
			Assert.AreEqual(1, l.Renderers.Count);
			var pad = ((SimpleLayout)l).Renderers[0] as PaddingLayoutRendererWrapper;
			Assert.IsNotNull(pad);
			var upperCase = ((SimpleLayout)pad.Inner).Renderers[0] as UppercaseLayoutRendererWrapper;
			Assert.IsNotNull(upperCase);
			var message = ((SimpleLayout)upperCase.Inner).Renderers[0] as MessageLayoutRenderer;
			Assert.IsNotNull(message);
		}

		[Test]
		public void EscapeTest()
		{
			AssertEscapeRoundTrips(string.Empty);
			AssertEscapeRoundTrips("hello ${${}} world!");
			AssertEscapeRoundTrips("hello $");
			AssertEscapeRoundTrips("hello ${");
			AssertEscapeRoundTrips("hello $${{");
			AssertEscapeRoundTrips("hello ${message}");
			AssertEscapeRoundTrips("hello ${${level}}");
			AssertEscapeRoundTrips("hello ${${level}${message}}");
		}

		[Test]
		public void EvaluateTest()
		{
			Assert.AreEqual("Off", CommonCfg.EvaluateLayout("${level}"));
			Assert.AreEqual(string.Empty, CommonCfg.EvaluateLayout("${message}"));
			Assert.AreEqual(string.Empty, CommonCfg.EvaluateLayout("${logger}"));
		}

		private void AssertEscapeRoundTrips(string originalString)
		{
			string escapedString = SimpleLayout.Escape(originalString);
			SimpleLayout l = escapedString;
			l.Initialize(CommonCfg);
			string renderedString = l.Render(LogEventInfo.CreateNullEvent());
			Assert.AreEqual(originalString, renderedString);
		}
	}
}
