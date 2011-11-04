using System;
using System.Text;
using System.Windows.Forms;

namespace NLog.Internal
{
	/// <summary>
	/// Message Box helper.
	/// </summary>
	internal class MessageBoxHelper
	{
		/// <summary>
		/// Shows the specified message using platform-specific message box.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="caption">The caption.</param>
		public static void Show(string message, string caption)
		{
			MessageBox.Show(message, caption);
		}
	}
}
