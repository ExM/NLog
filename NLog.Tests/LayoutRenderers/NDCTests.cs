using System;
using System.Xml;
using System.Reflection;
using NLog;
using NLog.Config;
using NUnit.Framework;

namespace NLog.UnitTests.LayoutRenderers
{
	[TestFixture]
	public class NDCTests : NLogTestBase
	{
		[Test]
		public void NDCTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${ndc} ${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			NestedDiagnosticsContext.Clear();
			LogManager.GetLogger("A").Debug("0");
			AssertDebugLastMessage("debug", " 0");
			using (NestedDiagnosticsContext.Push("ala"))
			{
				LogManager.GetLogger("A").Debug("a");
				AssertDebugLastMessage("debug", "ala a");
				using (NestedDiagnosticsContext.Push("ma"))
				{
					LogManager.GetLogger("A").Debug("b");
					AssertDebugLastMessage("debug", "ala ma b");
					using (NestedDiagnosticsContext.Push("kota"))
					{
						LogManager.GetLogger("A").Debug("c");
						AssertDebugLastMessage("debug", "ala ma kota c");
						using (NestedDiagnosticsContext.Push("kopytko"))
						{
							LogManager.GetLogger("A").Debug("d");
							AssertDebugLastMessage("debug", "ala ma kota kopytko d");
						}
						LogManager.GetLogger("A").Debug("c");
						AssertDebugLastMessage("debug", "ala ma kota c");
					}
					LogManager.GetLogger("A").Debug("b");
					AssertDebugLastMessage("debug", "ala ma b");
				}
				LogManager.GetLogger("A").Debug("a");
				AssertDebugLastMessage("debug", "ala a");
			}
			LogManager.GetLogger("A").Debug("0");
			AssertDebugLastMessage("debug", " 0");
		}

		[Test]
		public void NDCTopTestTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${ndc:topframes=2} ${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			NestedDiagnosticsContext.Clear();
			LogManager.GetLogger("A").Debug("0");
			AssertDebugLastMessage("debug", " 0");
			using (NestedDiagnosticsContext.Push("ala"))
			{
				LogManager.GetLogger("A").Debug("a");
				AssertDebugLastMessage("debug", "ala a");
				using (NestedDiagnosticsContext.Push("ma"))
				{
					LogManager.GetLogger("A").Debug("b");
					AssertDebugLastMessage("debug", "ala ma b");
					using (NestedDiagnosticsContext.Push("kota"))
					{
						LogManager.GetLogger("A").Debug("c");
						AssertDebugLastMessage("debug", "ma kota c");
						using (NestedDiagnosticsContext.Push("kopytko"))
						{
							LogManager.GetLogger("A").Debug("d");
							AssertDebugLastMessage("debug", "kota kopytko d");
						}
						LogManager.GetLogger("A").Debug("c");
						AssertDebugLastMessage("debug", "ma kota c");
					}
					LogManager.GetLogger("A").Debug("b");
					AssertDebugLastMessage("debug", "ala ma b");
				}
				LogManager.GetLogger("A").Debug("a");
				AssertDebugLastMessage("debug", "ala a");
			}
			LogManager.GetLogger("A").Debug("0");
			AssertDebugLastMessage("debug", " 0");
		}


		[Test]
		public void NDCTop1TestTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${ndc:topframes=1} ${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			NestedDiagnosticsContext.Clear();
			LogManager.GetLogger("A").Debug("0");
			AssertDebugLastMessage("debug", " 0");
			using (NestedDiagnosticsContext.Push("ala"))
			{
				LogManager.GetLogger("A").Debug("a");
				AssertDebugLastMessage("debug", "ala a");
				using (NestedDiagnosticsContext.Push("ma"))
				{
					LogManager.GetLogger("A").Debug("b");
					AssertDebugLastMessage("debug", "ma b");
					using (NestedDiagnosticsContext.Push("kota"))
					{
						LogManager.GetLogger("A").Debug("c");
						AssertDebugLastMessage("debug", "kota c");
						NestedDiagnosticsContext.Push("kopytko");
						LogManager.GetLogger("A").Debug("d");
						AssertDebugLastMessage("debug", "kopytko d");
						Assert.AreEqual("kopytko", NestedDiagnosticsContext.Pop()); // manual pop
						LogManager.GetLogger("A").Debug("c");
						AssertDebugLastMessage("debug", "kota c");
					}
					LogManager.GetLogger("A").Debug("b");
					AssertDebugLastMessage("debug", "ma b");
				}
				LogManager.GetLogger("A").Debug("a");
				AssertDebugLastMessage("debug", "ala a");
			}
			LogManager.GetLogger("A").Debug("0");
			AssertDebugLastMessage("debug", " 0");
			Assert.AreEqual(string.Empty, NestedDiagnosticsContext.Pop());
			Assert.AreEqual(string.Empty, NestedDiagnosticsContext.TopMessage);
			NestedDiagnosticsContext.Push("zzz");
			Assert.AreEqual("zzz", NestedDiagnosticsContext.TopMessage);
			NestedDiagnosticsContext.Clear();
			Assert.AreEqual(string.Empty, NestedDiagnosticsContext.Pop());
			Assert.AreEqual(string.Empty, NestedDiagnosticsContext.TopMessage);
		}

		[Test]
		public void NDCBottomTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${ndc:bottomframes=2} ${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			NestedDiagnosticsContext.Clear();
			LogManager.GetLogger("A").Debug("0");
			AssertDebugLastMessage("debug", " 0");
			using (NestedDiagnosticsContext.Push("ala"))
			{
				LogManager.GetLogger("A").Debug("a");
				AssertDebugLastMessage("debug", "ala a");
				using (NestedDiagnosticsContext.Push("ma"))
				{
					LogManager.GetLogger("A").Debug("b");
					AssertDebugLastMessage("debug", "ala ma b");
					using (NestedDiagnosticsContext.Push("kota"))
					{
						LogManager.GetLogger("A").Debug("c");
						AssertDebugLastMessage("debug", "ala ma c");
						using (NestedDiagnosticsContext.Push("kopytko"))
						{
							LogManager.GetLogger("A").Debug("d");
							AssertDebugLastMessage("debug", "ala ma d");
						}
						LogManager.GetLogger("A").Debug("c");
						AssertDebugLastMessage("debug", "ala ma c");
					}
					LogManager.GetLogger("A").Debug("b");
					AssertDebugLastMessage("debug", "ala ma b");
				}
				LogManager.GetLogger("A").Debug("a");
				AssertDebugLastMessage("debug", "ala a");
			}
			LogManager.GetLogger("A").Debug("0");
			AssertDebugLastMessage("debug", " 0");
		}

		[Test]
		public void NDCSeparatorTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${ndc:separator=\:} ${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			NestedDiagnosticsContext.Clear();
			LogManager.GetLogger("A").Debug("0");
			AssertDebugLastMessage("debug", " 0");
			using (NestedDiagnosticsContext.Push("ala"))
			{
				LogManager.GetLogger("A").Debug("a");
				AssertDebugLastMessage("debug", "ala a");
				using (NestedDiagnosticsContext.Push("ma"))
				{
					LogManager.GetLogger("A").Debug("b");
					AssertDebugLastMessage("debug", "ala:ma b");
					using (NestedDiagnosticsContext.Push("kota"))
					{
						LogManager.GetLogger("A").Debug("c");
						AssertDebugLastMessage("debug", "ala:ma:kota c");
						using (NestedDiagnosticsContext.Push("kopytko"))
						{
							LogManager.GetLogger("A").Debug("d");
							AssertDebugLastMessage("debug", "ala:ma:kota:kopytko d");
						}
						LogManager.GetLogger("A").Debug("c");
						AssertDebugLastMessage("debug", "ala:ma:kota c");
					}
					LogManager.GetLogger("A").Debug("b");
					AssertDebugLastMessage("debug", "ala:ma b");
				}
				LogManager.GetLogger("A").Debug("a");
				AssertDebugLastMessage("debug", "ala a");
			}
			LogManager.GetLogger("A").Debug("0");
			AssertDebugLastMessage("debug", " 0");
		}

	}
}