using System.Net.Mail;
using System;
using NLog.Common;

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
