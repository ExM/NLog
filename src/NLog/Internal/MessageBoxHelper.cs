
namespace NLog.Internal
{
	using System;
	using System.Text;
#if WINDOWS_PHONE
	using System.Windows;
#elif SILVERLIGHT
	using System.Windows;
	using System.Windows.Browser;
#else
	using System.Windows.Forms;
#endif

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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Not important here.")]
		public static void Show(string message, string caption)
		{
#if WINDOWS_PHONE
			MessageBox.Show(message, caption, MessageBoxButton.OK);
#elif SILVERLIGHT
			Action action = () => HtmlPage.Window.Alert(caption + "\r\n\r\n" + message);

			if (!Deployment.Current.Dispatcher.CheckAccess())
			{
				Deployment.Current.Dispatcher.BeginInvoke(action);
			}
			else
			{
				action();
			}
#else
			MessageBox.Show(message, caption);
#endif
		}
	}
}
