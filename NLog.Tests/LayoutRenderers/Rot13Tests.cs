using System;
using System.Diagnostics;
using System.Xml;
using System.Reflection;
using NLog;
using NLog.Config;
using NUnit.Framework;
using NLog.LayoutRenderers;
using NLog.Targets;
using NLog.Layouts;
using NLog.LayoutRenderers.Wrappers;

namespace NLog.UnitTests.LayoutRenderers
{
	[TestFixture]
	public class Rot13Tests : NLogTestBase
	{
		[Test]
		public void Test1()
		{
			Assert.AreEqual("NOPQRSTUVWXYZABCDEFGHIJKLM",
					Rot13LayoutRendererWrapper.DecodeRot13("ABCDEFGHIJKLMNOPQRSTUVWXYZ"));
			Assert.AreEqual("nopqrstuvwxyzabcdefghijklm0123456789",
					Rot13LayoutRendererWrapper.DecodeRot13("abcdefghijklmnopqrstuvwxyz0123456789"));
			Assert.AreEqual("How can you tell an extrovert from an introvert at NSA? Va gur ryringbef, gur rkgebiregf ybbx ng gur BGURE thl'f fubrf.",
			Rot13LayoutRendererWrapper.DecodeRot13(
							"Ubj pna lbh gryy na rkgebireg sebz na vagebireg ng AFN? In the elevators, the extroverts look at the OTHER guy's shoes."));
		}

		[Test]
		public void Test2()
		{
			Layout l = "${rot13:HELLO}";
			LogEventInfo lei = LogEventInfo.Create(LogLevel.Info, "aaa", "bbb");
			Assert.AreEqual("URYYB", l.Render(lei));
		}

		[Test]
		public void Test3()
		{
			Layout l = "${rot13:text=HELLO}";
			LogEventInfo lei = LogEventInfo.Create(LogLevel.Info, "aaa", "bbb");
			Assert.AreEqual("URYYB", l.Render(lei));
		}

		[Test]
		public void Test4()
		{
			Layout l = "${rot13:${event-context:aaa}}";
			LogEventInfo lei = LogEventInfo.Create(LogLevel.Info, "aaa", "bbb");
			lei.Properties["aaa"] = "HELLO";
			Assert.AreEqual("URYYB", l.Render(lei));
		}

		[Test]
		public void Test5()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
				<nlog>
					<targets>
						<target name='debug' type='Debug' layout='${rot13:${mdc:A}}' />
						<target name='debug2' type='Debug' layout='${rot13:${rot13:${mdc:A}}}' />
					 </targets>
					<rules>
						<logger name='*' levels='Trace' writeTo='debug,debug2' />
					</rules>
				</nlog>");

			MappedDiagnosticsContext.Set("A", "Foo.Bar!");
			Logger l = LogManager.GetLogger("NLog.UnitTests.LayoutRenderers.Rot13Tests");
			l.Trace("aaa");

			AssertDebugLastMessage("debug", "Sbb.One!");

			// double rot-13 should be identity
			AssertDebugLastMessage("debug2", "Foo.Bar!");
		}
	}
}