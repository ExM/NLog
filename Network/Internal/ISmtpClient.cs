using System;
using System.Net;
using System.Net.Mail;

namespace NLog.Internal
{
	/// <summary>
	/// Supports mocking of SMTP Client code.
	/// </summary>
	public interface ISmtpClient : IDisposable //TODO: change namespace
	{
		string Host { get; set; }

		int Port { get; set; }

		ICredentialsByHost Credentials { get; set; }

		bool EnableSsl { get; set; }

		void Send(MailMessage msg);
	}
}
