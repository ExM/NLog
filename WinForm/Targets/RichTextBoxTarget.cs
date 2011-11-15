using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NLog.Config;
using NLog.Internal;
using NLog.Common;

namespace NLog.Targets
{

	/// <summary>
	/// Log text a Rich Text Box control in an existing or new form.
	/// </summary>
	/// <seealso href="http://nlog-project.org/wiki/RichTextBox_target">Documentation on NLog Wiki</seealso>
	/// <example>
	/// <p>
	/// To set up the target in the <a href="config.html">configuration file</a>, 
	/// use the following syntax:
	/// </p><code lang="XML" source="examples/targets/Configuration File/RichTextBox/Simple/NLog.config">
	/// </code>
	/// <p>
	/// The result is:
	/// </p><img src="examples/targets/Screenshots/RichTextBox/Simple.gif"/><p>
	/// To set up the target with coloring rules in the <a href="config.html">configuration file</a>, 
	/// use the following syntax:
	/// </p><code lang="XML" source="examples/targets/Configuration File/RichTextBox/RowColoring/NLog.config">
	/// </code>
	/// <code lang="XML" source="examples/targets/Configuration File/RichTextBox/WordColoring/NLog.config">
	/// </code>
	/// <p>
	/// The result is:
	/// </p><img src="examples/targets/Screenshots/RichTextBox/RowColoring.gif"/><img src="examples/targets/Screenshots/RichTextBox/WordColoring.gif"/><p>
	/// To set up the log target programmatically similar to above use code like this:
	/// </p><code lang="C#" source="examples/targets/Configuration API/RichTextBox/Simple/Form1.cs">
	/// </code>
	/// ,
	/// <code lang="C#" source="examples/targets/Configuration API/RichTextBox/RowColoring/Form1.cs">
	/// </code>
	/// for RowColoring,
	/// <code lang="C#" source="examples/targets/Configuration API/RichTextBox/WordColoring/Form1.cs">
	/// </code>
	/// for WordColoring
	/// </example>
	[Target("RichTextBox")]
	public sealed class RichTextBoxTarget : TargetWithLayout, ISupportsLazyParameters
	{
		private int lineCount;

		/// <summary>
		/// Initializes static members of the RichTextBoxTarget class.
		/// </summary>
		/// <remarks>
		/// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
		/// </remarks>
		static RichTextBoxTarget()
		{
		}

		private List<RichTextBoxRowColoringRule> CreateDefautRules()
		{
			return new List<RichTextBoxRowColoringRule>()
			{
				new RichTextBoxRowColoringRule("level == LogLevel.Fatal", "White", "Red", FontStyle.Bold),
				new RichTextBoxRowColoringRule("level == LogLevel.Error", "Red", "Empty", FontStyle.Bold | FontStyle.Italic),
				new RichTextBoxRowColoringRule("level == LogLevel.Warn", "Orange", "Empty", FontStyle.Underline),
				new RichTextBoxRowColoringRule("level == LogLevel.Info", "Black", "Empty"),
				new RichTextBoxRowColoringRule("level == LogLevel.Debug", "Gray", "Empty"),
				new RichTextBoxRowColoringRule("level == LogLevel.Trace", "DarkGray", "Empty", FontStyle.Italic),
			};
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RichTextBoxTarget" /> class.
		/// </summary>
		/// <remarks>
		/// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
		/// </remarks>
		public RichTextBoxTarget()
		{
			this.WordColoringRules = new List<RichTextBoxWordColoringRule>();
			this.RowColoringRules = new List<RichTextBoxRowColoringRule>();
			this.ToolWindow = true;
		}

		private delegate void DelSendTheMessageToRichTextBox(string logMessage, RichTextBoxRowColoringRule rule);

		private delegate void FormCloseDelegate();

		/// <summary>
		/// Gets or sets the Name of RichTextBox to which Nlog will write.
		/// </summary>
		/// <docgen category='Form Options' order='10' />
		public string ControlName { get; set; }

		/// <summary>
		/// Gets or sets the name of the Form on which the control is located. 
		/// If there is no open form of a specified name than NLog will create a new one.
		/// </summary>
		/// <docgen category='Form Options' order='10' />
		public string FormName { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to use default coloring rules.
		/// </summary>
		/// <docgen category='Highlighting Options' order='10' />
		[DefaultValue(false)]
		public bool UseDefaultRowColoringRules { get; set; }

		/// <summary>
		/// Gets the row coloring rules.
		/// </summary>
		/// <docgen category='Highlighting Options' order='10' />
		[ArrayParameter(typeof(RichTextBoxRowColoringRule), "row-coloring")]
		public IList<RichTextBoxRowColoringRule> RowColoringRules { get; private set; }

		/// <summary>
		/// The default row coloring rules
		/// </summary>
		public IList<RichTextBoxRowColoringRule> DefaultRowColoringRules { get; private set; }

		/// <summary>
		/// Gets the word highlighting rules.
		/// </summary>
		/// <docgen category='Highlighting Options' order='10' />
		[ArrayParameter(typeof(RichTextBoxWordColoringRule), "word-coloring")]
		public IList<RichTextBoxWordColoringRule> WordColoringRules { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether the created window will be a tool window.
		/// </summary>
		/// <remarks>
		/// This parameter is ignored when logging to existing form control.
		/// Tool windows have thin border, and do not show up in the task bar.
		/// </remarks>
		/// <docgen category='Form Options' order='10' />
		[DefaultValue(true)]
		public bool ToolWindow { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the created form will be initially minimized.
		/// </summary>
		/// <remarks>
		/// This parameter is ignored when logging to existing form control.
		/// </remarks>
		/// <docgen category='Form Options' order='10' />
		public bool ShowMinimized { get; set; }

		/// <summary>
		/// Gets or sets the initial width of the form with rich text box.
		/// </summary>
		/// <remarks>
		/// This parameter is ignored when logging to existing form control.
		/// </remarks>
		/// <docgen category='Form Options' order='10' />
		public int Width { get; set; }

		/// <summary>
		/// Gets or sets the initial height of the form with rich text box.
		/// </summary>
		/// <remarks>
		/// This parameter is ignored when logging to existing form control.
		/// </remarks>
		/// <docgen category='Form Options' order='10' />
		public int Height { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether scroll bar will be moved automatically to show most recent log entries.
		/// </summary>
		/// <docgen category='Form Options' order='10' />
		public bool AutoScroll { get; set; }

		/// <summary>
		/// Gets or sets the maximum number of lines the rich text box will store (or 0 to disable this feature).
		/// </summary>
		/// <remarks>
		/// After exceeding the maximum number, first line will be deleted. 
		/// </remarks>
		/// <docgen category='Form Options' order='10' />
		public int MaxLines { get; set; }

		/// <summary>
		/// Gets or sets the form to log to.
		/// </summary>
		public Form TargetForm { get; set; }

		/// <summary>
		/// Gets or sets the rich text box to log to.
		/// </summary>
		public RichTextBox TargetRichTextBox { get; set; }

		public bool CreatedForm { get; set; }

		/// <summary>
		/// Initializes the target. Can be used by inheriting classes
		/// to initialize logging.
		/// </summary>
		protected override void InitializeTarget()
		{
			base.InitializeTarget();
			if (this.FormName == null)
			{
				this.FormName = "NLogForm" + Guid.NewGuid().ToString("N");
			}

			var openFormByName = Application.OpenForms[this.FormName];
			if (openFormByName != null)
			{
				this.TargetForm = openFormByName;
				if (string.IsNullOrEmpty(this.ControlName))
				{
					throw new NLogConfigurationException("Rich text box control name must be specified for " + this.GetType().Name + ".");
				}

				this.CreatedForm = false;
				this.TargetRichTextBox = FormHelper.FindControl<RichTextBox>(this.ControlName, this.TargetForm);

				if (this.TargetRichTextBox == null)
				{
					throw new NLogConfigurationException("Rich text box control '" + this.ControlName + "' cannot be found on form '" + this.FormName + "'.");
				}
			}
			else
			{
				this.TargetForm = FormHelper.CreateForm(this.FormName, this.Width, this.Height, true, this.ShowMinimized, this.ToolWindow);
				this.TargetRichTextBox = FormHelper.CreateRichTextBox(this.ControlName, this.TargetForm);
				this.CreatedForm = true;
			}
		}

		/// <summary>
		/// Closes the target and releases any unmanaged resources.
		/// </summary>
		protected override void CloseTarget()
		{
			if (this.CreatedForm)
			{
				this.TargetForm.BeginInvoke((FormCloseDelegate)this.TargetForm.Close);
				this.TargetForm = null;
			}
		}

		/// <summary>
		/// Log message to RichTextBox.
		/// </summary>
		/// <param name="logEvent">The logging event.</param>
		protected override void Write(LogEventInfo logEvent)
		{
			RichTextBoxRowColoringRule matchingRule = null;

			foreach (RichTextBoxRowColoringRule rr in this.RowColoringRules)
			{
				if (rr.CheckCondition(logEvent))
				{
					matchingRule = rr;
					break;
				}
			}

			if (this.UseDefaultRowColoringRules && matchingRule == null)
			{
				foreach (RichTextBoxRowColoringRule rr in DefaultRowColoringRules)
				{
					if (rr.CheckCondition(logEvent))
					{
						matchingRule = rr;
						break;
					}
				}
			}

			if (matchingRule == null)
			{
				matchingRule = RichTextBoxRowColoringRule.Default;
			}
			
			string logMessage = this.Layout.Render(logEvent);

			this.TargetRichTextBox.BeginInvoke(new DelSendTheMessageToRichTextBox(this.SendTheMessageToRichTextBox), new object[] { logMessage, matchingRule });
		}

		private static Color GetColorFromString(string color, Color defaultColor)
		{
			if (color == "Empty")
			{
				return defaultColor;
			}

			return Color.FromName(color);
		}

		private void SendTheMessageToRichTextBox(string logMessage, RichTextBoxRowColoringRule rule)
		{
			RichTextBox rtbx = this.TargetRichTextBox;

			int startIndex = rtbx.Text.Length;
			rtbx.SelectionStart = startIndex;
			rtbx.SelectionBackColor = GetColorFromString(rule.BackgroundColor, rtbx.BackColor);
			rtbx.SelectionColor = GetColorFromString(rule.FontColor, rtbx.ForeColor);
			rtbx.SelectionFont = new Font(rtbx.SelectionFont, rtbx.SelectionFont.Style ^ rule.Style);
			rtbx.AppendText(logMessage + "\n");
			rtbx.SelectionLength = rtbx.Text.Length - rtbx.SelectionStart;

			// find word to color
			foreach (RichTextBoxWordColoringRule wordRule in this.WordColoringRules)
			{
				MatchCollection mc = wordRule.CompiledRegex.Matches(rtbx.Text, startIndex);
				foreach (Match m in mc)
				{
					rtbx.SelectionStart = m.Index;
					rtbx.SelectionLength = m.Length;
					rtbx.SelectionBackColor = GetColorFromString(wordRule.BackgroundColor, rtbx.BackColor);
					rtbx.SelectionColor = GetColorFromString(wordRule.FontColor, rtbx.ForeColor);
					rtbx.SelectionFont = new Font(rtbx.SelectionFont, rtbx.SelectionFont.Style ^ wordRule.Style);
				}
			}

			if (this.MaxLines > 0)
			{
				this.lineCount++;
				if (this.lineCount > this.MaxLines)
				{
					int pos = rtbx.GetFirstCharIndexFromLine(1);
					rtbx.Select(0, pos);
					rtbx.SelectedText = string.Empty;
					this.lineCount--;
				}
			}

			if (this.AutoScroll)
			{
				rtbx.Select(rtbx.TextLength, 0);
				rtbx.ScrollToCaret();
			}
		}

		public void CreateParameters(LoggingConfiguration cfg)
		{
			if (UseDefaultRowColoringRules)
				DefaultRowColoringRules = CreateDefautRules();
		}
	}
}

