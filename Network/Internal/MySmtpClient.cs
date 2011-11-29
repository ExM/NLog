using System.Net.Mail;
using System;

namespace NLog.Internal
{
	/// <summary>
	/// Supports mocking of SMTP Client code.
	/// </summary>
	public class MySmtpClient : SmtpClient, ISmtpClient, IDisposable
	{
		public void Dispose()
		{
		}
	}
}
