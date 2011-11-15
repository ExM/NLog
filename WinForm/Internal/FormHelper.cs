using System;
using System.Drawing;
using System.Windows.Forms;

namespace NLog.Internal
{
	/// <summary>
	/// Form helper methods.
	/// </summary>
	internal class FormHelper
	{
		/// <summary>
		/// Creates RichTextBox and docks in parentForm.
		/// </summary>
		/// <param name="name">Name of RichTextBox.</param>
		/// <param name="parentForm">Form to dock RichTextBox.</param>
		/// <returns>Created RichTextBox.</returns>
		internal static RichTextBox CreateRichTextBox(string name, Form parentForm)
		{
			var rtb = new RichTextBox();
			rtb.Dock = System.Windows.Forms.DockStyle.Fill;
			rtb.Location = new System.Drawing.Point(0, 0);
			rtb.Name = name;
			rtb.Size = new System.Drawing.Size(parentForm.Width, parentForm.Height);
			parentForm.Controls.Add(rtb);
			return rtb;
		}

		/// <summary>
		/// Finds control embedded on searchControl.
		/// </summary>
		/// <param name="name">Name of the control.</param>
		/// <param name="searchControl">Control in which we're searching for control.</param>
		/// <returns>A value of null if no control has been found.</returns>
		internal static Control FindControl(string name, Control searchControl)
		{
			if (searchControl.Name == name)
			{
				return searchControl;
			}

			foreach (Control childControl in searchControl.Controls)
			{
				Control foundControl = FindControl(name, childControl);
				if (foundControl != null)
				{
					return foundControl;
				}
			}

			return null;
		}

		/// <summary>
		/// Finds control of specified type embended on searchControl.
		/// </summary>
		/// <typeparam name="TControl">The type of the control.</typeparam>
		/// <param name="name">Name of the control.</param>
		/// <param name="searchControl">Control in which we're searching for control.</param>
		/// <returns>
		/// A value of null if no control has been found.
		/// </returns>
		internal static TControl FindControl<TControl>(string name, Control searchControl)
			where TControl : Control
		{
			if (searchControl.Name == name)
			{
				TControl foundControl = searchControl as TControl;
				if (foundControl != null)
				{
					return foundControl;
				}
			}

			foreach (Control childControl in searchControl.Controls)
			{
				TControl foundControl = FindControl<TControl>(name, childControl);

				if (foundControl != null)
				{
					return foundControl;
				}
			}

			return null;
		}

		/// <summary>
		/// Creates a form.
		/// </summary>
		/// <param name="name">Name of form.</param>
		/// <param name="width">Width of form.</param>
		/// <param name="height">Height of form.</param>
		/// <param name="show">Auto show form.</param>
		/// <param name="showMinimized">If set to <c>true</c> the form will be minimized.</param>
		/// <param name="toolWindow">If set to <c>true</c> the form will be created as tool window.</param>
		/// <returns>Created form.</returns>
		internal static Form CreateForm(string name, int width, int height, bool show, bool showMinimized, bool toolWindow)
		{
			var f = new Form
			{
				Name = name,
				Text = "NLog",
				Icon = GetNLogIcon()
			};

			if (toolWindow)
			{
				f.FormBorderStyle = FormBorderStyle.SizableToolWindow;
			}

			if (width > 0)
			{
				f.Width = width;
			}

			if (height > 0)
			{
				f.Height = height;
			}

			if (show)
			{
				if (showMinimized)
				{
					f.WindowState = FormWindowState.Minimized;
					f.Show();
				}
				else
				{
					f.Show();
				}
			}

			return f;
		}

		private static Icon GetNLogIcon()
		{
			using (var stream = typeof(FormHelper).Assembly.GetManifestResourceStream("NLog.WinForm.Resources.NLog.ico"))
			{
				return new Icon(stream);
			}
		}
	}
}

