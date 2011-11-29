using System.Diagnostics;
using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using NLog.Internal;
using NLog.WinForm.RtfParsing;
using System.Collections.Generic;

namespace NLog.UnitTests.Targets
{

	[TestFixture]
	public class RichTextBoxTargetTests : NLogTestBase
	{
		private Logger logger = LogManager.GetLogger("NLog.UnitTests.Targets.RichTextBoxTargetTests");

		[Test]
		public void SimpleRichTextBoxTargetTest()
		{
			RichTextBoxTarget target = new RichTextBoxTarget()
			{
				ControlName = "Control1",
				UseDefaultRowColoringRules = true,
				Layout = "${level} ${logger} ${message}",
				ToolWindow = false,
				Width = 300,
				Height = 200,
			};

			SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
			logger.Fatal(" {Test}rrr");
			logger.Error(" }F;o\\o");
			logger.Warn("Bar");
			logger.Info("Test");
			logger.Debug("Foo");
			logger.Trace("Bar");

			Application.DoEvents();

			var form = target.TargetForm;

			Assert.IsTrue(target.CreatedForm);
			Assert.IsTrue(form.Name.StartsWith("NLog"));
			Assert.AreEqual(FormWindowState.Normal, form.WindowState);
			Assert.AreEqual("NLog", form.Text);
			Assert.AreEqual(300, form.Width);
			Assert.AreEqual(200, form.Height);

			MemoryStream ms = new MemoryStream();
			target.TargetRichTextBox.SaveFile(ms, RichTextBoxStreamType.RichText);
			string rtfText = Encoding.UTF8.GetString(ms.GetBuffer());

			Assert.IsTrue(target.CreatedForm);
			Console.WriteLine(rtfText);
			List<RtfParagraph> phs = RtfDocument.Load(rtfText);
/*
{\rtf1\ansi\ansicpg1251\deff0\deflang1049{\fonttbl{\f0\fnil\fcharset204 Microsoft Sans Serif;}}
{\colortbl ;\red255\green255\blue255;\red255\green0\blue0;\red255\green165\blue0;\red0\green0\blue0;\red128\green128\blue128;\red169\green169\blue169;}
\viewkind4\uc1\pard\cf1\highlight2\b\f0\fs17 Fatal NLog.UnitTests.Targets.RichTextBoxTargetTests  \{Test\}rrr\par
\cf2\highlight1\i Error NLog.UnitTests.Targets.RichTextBoxTargetTests  \}F;o\\o\par
\cf3\ul\b0\i0 Warn NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par
\cf4\ulnone Info NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par
\cf5 Debug NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par
\cf6\i Trace NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par
\cf0\highlight0\i0\par
}
*/
			Color bc = target.TargetRichTextBox.BackColor;
			Color w = Color.FromName("White");
			Color r = Color.FromName("Red");
			Color o = Color.FromName("Orange");
			Color b = Color.FromName("Black");
			Color g = Color.FromName("Gray");
			Color dg = Color.FromName("DarkGray");

			CheckText(phs[0][0], "Fatal NLog.UnitTests.Targets.RichTextBoxTargetTests  {Test}rrr", FontStyle.Bold, w, r);
			CheckText(phs[1][0], "Error NLog.UnitTests.Targets.RichTextBoxTargetTests  }F;o\\o", FontStyle.Bold | FontStyle.Italic, r, bc);
			CheckText(phs[2][0], "Warn NLog.UnitTests.Targets.RichTextBoxTargetTests Bar", FontStyle.Underline, o, bc);
			CheckText(phs[3][0], "Info NLog.UnitTests.Targets.RichTextBoxTargetTests Test", FontStyle.Regular, b, bc);
			CheckText(phs[4][0], "Debug NLog.UnitTests.Targets.RichTextBoxTargetTests Foo", FontStyle.Regular, g, bc);
			CheckText(phs[5][0], "Trace NLog.UnitTests.Targets.RichTextBoxTargetTests Bar", FontStyle.Italic, dg, bc);

			LogManager.Configuration = null;
			Assert.IsNull(target.TargetForm);
			Application.DoEvents();
			Assert.IsTrue(form.IsDisposed);
		}

		public void CheckText(RtfText rtf, string exText, FontStyle exfs, Color? exF = null, Color? exB = null)
		{
			Assert.AreEqual(exText, rtf.Text);
			Assert.AreEqual(exfs, rtf.FontStyle);
			if(exF != null)
				Assert.AreEqual(exF.Value.ToArgb(), rtf.FColor.ToArgb(), "{0} not equal {1} ", exF, rtf.FColor);
			if(!InMono && exB != null) // mono not set background color
				Assert.AreEqual(exB.Value.ToArgb(), rtf.BColor.ToArgb(), "{0} not equal {1} ", exB, rtf.BColor);
		}

		[Test]
		public void NoColoringTest()
		{
			try
			{
				RichTextBoxTarget target = new RichTextBoxTarget()
				{
					ControlName = "Control1",
					Layout = "${level} ${message}",
					ShowMinimized = true,
					ToolWindow = false,
				};

				SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
				logger.Fatal("Test");
				logger.Error("Foo");
				logger.Warn("Bar");
				logger.Info("Test");
				logger.Debug("Foo");
				logger.Trace("Bar");

				Application.DoEvents();

				MemoryStream ms = new MemoryStream();
				target.TargetRichTextBox.SaveFile(ms, RichTextBoxStreamType.RichText);
				string rtfText = Encoding.UTF8.GetString(ms.GetBuffer());
				
				Assert.IsTrue(target.CreatedForm);

				Console.WriteLine(rtfText);
				List<RtfParagraph> phs = RtfDocument.Load(rtfText);
/*
{\rtf1\ansi\ansicpg1251\deff0\deflang1049{\fonttbl{\f0\fnil\fcharset204 Microsoft Sans Serif;}}
{\colortbl ;\red0\green0\blue0;\red255\green255\blue255;}
\viewkind4\uc1\pard\cf1\highlight2\f0\fs17 Fatal Test\par
Error Foo\par
Warn Bar\par
Info Test\par
Debug Foo\par
Trace Bar\par
\cf0\highlight0\par
}
*/
				Color fc = target.TargetRichTextBox.ForeColor;
				Color bc = target.TargetRichTextBox.BackColor;

				CheckText(phs[0][0], "Fatal Test", FontStyle.Regular, fc, bc);
				CheckText(phs[1][0], "Error Foo", FontStyle.Regular, fc, bc);
				CheckText(phs[2][0], "Warn Bar", FontStyle.Regular, fc, bc);
				CheckText(phs[3][0], "Info Test", FontStyle.Regular, fc, bc);
				CheckText(phs[4][0], "Debug Foo", FontStyle.Regular, fc, bc);
				CheckText(phs[5][0], "Trace Bar", FontStyle.Regular, fc, bc);
			}
			finally
			{
				LogManager.Configuration = null;
			}
		}

		[Test]
		public void CustomRowColoringTest()
		{
			try
			{
				RichTextBoxTarget target = new RichTextBoxTarget()
				{
					ControlName = "Control1",
					Layout = "${level} ${message}",
					ShowMinimized = true,
					ToolWindow = false,
					RowColoringRules =
					{
						new RichTextBoxRowColoringRule("starts-with(message, 'B')", "Maroon", "Empty"),
					}
				};

				SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
				logger.Fatal("Test");
				logger.Error("Foo");
				logger.Warn("Bar");
				logger.Info("Test");
				logger.Debug("Foo");
				logger.Trace("Bar");

				Application.DoEvents();

				MemoryStream ms = new MemoryStream();
				target.TargetRichTextBox.SaveFile(ms, RichTextBoxStreamType.RichText);
				string rtfText = Encoding.UTF8.GetString(ms.GetBuffer());

				Assert.IsTrue(target.CreatedForm);
				Console.WriteLine(rtfText);
/* win
{\rtf1\ansi\ansicpg1251\deff0\deflang1049{\fonttbl{\f0\fnil\fcharset204 Microsoft Sans Serif;}}
{\colortbl ;\red0\green0\blue0;\red255\green255\blue255;\red128\green0\blue0;}
\viewkind4\uc1\pard\cf1\highlight2\f0\fs17 Fatal Test\par
Error Foo\par
\cf3 Warn Bar\par
\cf1 Info Test\par
Debug Foo\par
\cf3 Trace Bar\par
\cf0\highlight0\par
}
*/
				List<RtfParagraph> phs = RtfDocument.Load(rtfText);

				Color fc = target.TargetRichTextBox.ForeColor;
				Color bc = target.TargetRichTextBox.BackColor;
				Color m = Color.FromName("Maroon");

				CheckText(phs[0][0], "Fatal Test", FontStyle.Regular, fc, bc);
				CheckText(phs[1][0], "Error Foo", FontStyle.Regular, fc, bc);
				CheckText(phs[2][0], "Warn Bar", FontStyle.Regular, m, bc);
				CheckText(phs[3][0], "Info Test", FontStyle.Regular, fc, bc);
				CheckText(phs[4][0], "Debug Foo", FontStyle.Regular, fc, bc);
				CheckText(phs[5][0], "Trace Bar", FontStyle.Regular, m, bc);
			}
			finally
			{
				LogManager.Configuration = null;
			}
		}

		[Test]
		public void CustomWordRowColoringTest()
		{
			try
			{
				RichTextBoxTarget target = new RichTextBoxTarget()
				{
					ControlName = "Control1",
					Layout = "${level} ${message}",
					ShowMinimized = true,
					ToolWindow = false,
					WordColoringRules =
					{
						new RichTextBoxWordColoringRule("zzz", "Red", "Empty"),
						new RichTextBoxWordColoringRule("aaa", "Green", "Empty"),
					}
				};

				SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
				logger.Fatal("Test zzz");
				logger.Error("Foo xxx");
				logger.Warn("Bar yyy");
				logger.Info("Test aaa");
				logger.Debug("Foo zzz");
				logger.Trace("Bar ccc");

				Application.DoEvents();

				MemoryStream ms = new MemoryStream();
				target.TargetRichTextBox.SaveFile(ms, RichTextBoxStreamType.RichText);
				string rtfText = Encoding.UTF8.GetString(ms.GetBuffer());

				Assert.IsTrue(target.CreatedForm);

				// "zzz" string will be highlighted

				Console.WriteLine(rtfText);
				List<RtfParagraph> phs = RtfDocument.Load(rtfText);
/*
{\rtf1\ansi\ansicpg1251\deff0\deflang1049{\fonttbl{\f0\fnil\fcharset204 Microsoft Sans Serif;}}
{\colortbl ;\red0\green0\blue0;\red255\green255\blue255;\red255\green0\blue0;\red0\green128\blue0;}
\viewkind4\uc1\pard\cf1\highlight2\f0\fs17 Fatal Test \cf3 zzz\cf1\par
Error Foo xxx\par
Warn Bar yyy\par
Info Test \cf4 aaa\cf1\par
Debug Foo \cf3 zzz\cf1\par
Trace Bar ccc\par
\cf0\highlight0\par
}
*/
				Color fc = target.TargetRichTextBox.ForeColor;
				Color bc = target.TargetRichTextBox.BackColor;
				Color r = Color.FromName("Red");
				Color g = Color.FromName("Green");

				CheckText(phs[0][0], "Fatal Test ", FontStyle.Regular, fc, bc); CheckText(phs[0][1], "zzz", FontStyle.Regular, r, bc);
				CheckText(phs[1][0], "Error Foo xxx", FontStyle.Regular, fc, bc);
				CheckText(phs[2][0], "Warn Bar yyy", FontStyle.Regular, fc, bc);
				CheckText(phs[3][0], "Info Test ", FontStyle.Regular, fc, bc); CheckText(phs[3][1], "aaa", FontStyle.Regular, g, bc);
				CheckText(phs[4][0], "Debug Foo ", FontStyle.Regular, fc, bc); CheckText(phs[4][1], "zzz", FontStyle.Regular, r, bc);
				CheckText(phs[5][0], "Trace Bar ccc", FontStyle.Regular, fc, bc);
			}
			finally
			{
				LogManager.Configuration = null;
			}
		}

		[Test]
		public void RichTextBoxTargetDefaultsTest()
		{
			var target = new RichTextBoxTarget();
			Assert.IsFalse(target.UseDefaultRowColoringRules);
			Assert.AreEqual(0, target.WordColoringRules.Count);
			Assert.AreEqual(0, target.RowColoringRules.Count);
			Assert.IsNull(target.FormName);
			Assert.IsNull(target.ControlName);
		}

		[Test]
		public void AutoScrollTest()
		{
			try
			{
				RichTextBoxTarget target = new RichTextBoxTarget()
				{
					ControlName = "Control1",
					Layout = "${level} ${logger} ${message}",
					ShowMinimized = true,
					ToolWindow = false,
					AutoScroll = true,
				};

				SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
				for (int i = 0; i < 100; ++i)
				{
					logger.Info("Test");
					Application.DoEvents();
					Assert.AreEqual(target.TargetRichTextBox.SelectionStart, target.TargetRichTextBox.TextLength);
					Assert.AreEqual(target.TargetRichTextBox.SelectionLength, 0);
				}
			}
			finally
			{
				LogManager.Configuration = null;
			}
		}

		[Test]
		public void MaxLinesTest()
		{
			try
			{
				RichTextBoxTarget target = new RichTextBoxTarget()
				{
					ControlName = "Control1",
					Layout = "${message}",
					ShowMinimized = true,
					ToolWindow = false,
					AutoScroll = true,
				};

				Assert.AreEqual(0, target.MaxLines);
				target.MaxLines = 7;

				SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
				for (int i = 0; i < 100; ++i)
				{
					logger.Info("Test {0}", i);
				}

				Application.DoEvents();
				string expectedText = "Test 93\nTest 94\nTest 95\nTest 96\nTest 97\nTest 98\nTest 99\n";

				Assert.AreEqual(expectedText, target.TargetRichTextBox.Text);
			}
			finally
			{
				LogManager.Configuration = null;
			}
		}

		[Test]
		public void ColoringRuleDefaults()
		{
			var expectedRules = new[]
			{
				new RichTextBoxRowColoringRule("level == LogLevel.Fatal", "White", "Red", FontStyle.Bold),
				new RichTextBoxRowColoringRule("level == LogLevel.Error", "Red", "Empty", FontStyle.Bold | FontStyle.Italic),
				new RichTextBoxRowColoringRule("level == LogLevel.Warn", "Orange", "Empty", FontStyle.Underline),
				new RichTextBoxRowColoringRule("level == LogLevel.Info", "Black", "Empty"),
				new RichTextBoxRowColoringRule("level == LogLevel.Debug", "Gray", "Empty"),
				new RichTextBoxRowColoringRule("level == LogLevel.Trace", "DarkGray", "Empty", FontStyle.Italic),
			};

			RichTextBoxTarget target = new RichTextBoxTarget()
			{
				ControlName = "Control1",
				UseDefaultRowColoringRules = true,
				Layout = "${level} ${logger} ${message}",
				ToolWindow = false,
				Width = 300,
				Height = 200,
			};

			((ISupportsLazyParameters)target).CreateParameters(CommonCfg);

			var actualRules = target.DefaultRowColoringRules;
			Assert.AreEqual(expectedRules.Length, actualRules.Count);
			for (int i = 0; i < expectedRules.Length; ++i)
			{
				Assert.AreEqual(expectedRules[i].BackgroundColor, actualRules[i].BackgroundColor);
				Assert.AreEqual(expectedRules[i].FontColor, actualRules[i].FontColor);
				Assert.AreEqual(expectedRules[i].Condition.ToString(), actualRules[i].Condition.ToString());
				Assert.AreEqual(expectedRules[i].Style, actualRules[i].Style);
			}
		}

		[Test]
		public void ActiveFormTest()
		{
			RichTextBoxTarget target = new RichTextBoxTarget()
			{
				FormName = "MyForm1",
				ControlName = "Control1",
				UseDefaultRowColoringRules = true,
				Layout = "${level} ${logger} ${message}",
				ToolWindow = false,
				Width = 300,
				Height = 200,
			};

			using (Form form = new Form())
			{
				form.Name = "MyForm1";
				form.WindowState = FormWindowState.Minimized;

				RichTextBox rtb = new RichTextBox();
				rtb.Dock = DockStyle.Fill;
				rtb.Name = "Control1";
				form.Controls.Add(rtb);
				form.Shown += (sender, e) =>
					{
						target.Initialize(CommonCfg);
						form.Activate();
						Application.DoEvents();
						Assert.AreSame(form, target.TargetForm);
						Assert.AreSame(rtb, target.TargetRichTextBox);
						form.Close();
					};

				form.ShowDialog();
				Application.DoEvents();
			}
		}

		[Test]
		public void ActiveFormTest2()
		{
			RichTextBoxTarget target = new RichTextBoxTarget()
			{
				FormName = "MyForm2",
				ControlName = "Control1",
				UseDefaultRowColoringRules = true,
				Layout = "${level} ${logger} ${message}",
				ToolWindow = false,
				Width = 300,
				Height = 200,
			};

			using (Form form = new Form())
			{
				form.Name = "MyForm1";
				form.WindowState = FormWindowState.Minimized;

				RichTextBox rtb = new RichTextBox();
				rtb.Dock = DockStyle.Fill;
				rtb.Name = "Control1";
				form.Controls.Add(rtb);
				form.Show();
				using (Form form1 = new Form())
				{
					form1.Name = "MyForm2";
					RichTextBox rtb2 = new RichTextBox();
					rtb2.Dock = DockStyle.Fill;
					rtb2.Name = "Control1";
					form1.Controls.Add(rtb2);
					form1.Show();
					form1.Activate();

					target.Initialize(CommonCfg);
					Assert.AreSame(form1, target.TargetForm);
					Assert.AreSame(rtb2, target.TargetRichTextBox);
				}
			}
		}

		[Test]
		public void ActiveFormNegativeTest1()
		{
			RichTextBoxTarget target = new RichTextBoxTarget()
			{
				FormName = "MyForm1",
				ControlName = "Control1",
				UseDefaultRowColoringRules = true,
				Layout = "${level} ${logger} ${message}",
				ToolWindow = false,
				Width = 300,
				Height = 200,
			};

			using (Form form = new Form())
			{
				form.Name = "MyForm1";
				form.WindowState = FormWindowState.Minimized;

				//RichTextBox rtb = new RichTextBox();
				//rtb.Dock = DockStyle.Fill;
				//rtb.Name = "Control1";
				//form.Controls.Add(rtb);
				form.Show();
				try
				{
					target.Initialize(CommonCfg);
					Assert.Fail("Expected exception.");
				}
				catch (NLogConfigurationException ex)
				{
					Assert.IsNotNull(ex.InnerException);
					Assert.AreEqual("Rich text box control 'Control1' cannot be found on form 'MyForm1'.", ex.InnerException.Message);
				}
			}
		}

		[Test]
		public void ActiveFormNegativeTest2()
		{
			RichTextBoxTarget target = new RichTextBoxTarget()
			{
				FormName = "MyForm1",
				UseDefaultRowColoringRules = true,
				Layout = "${level} ${logger} ${message}",
			};

			using (Form form = new Form())
			{
				form.Name = "MyForm1";
				form.WindowState = FormWindowState.Minimized;
				form.Show();

				try
				{
					target.Initialize(CommonCfg);
					Assert.Fail("Expected exception.");
				}
				catch (NLogConfigurationException ex)
				{
					Assert.IsNotNull(ex.InnerException);
					Assert.AreEqual("Rich text box control name must be specified for RichTextBoxTarget.", ex.InnerException.Message);
				}
			}
		}
	}
}
