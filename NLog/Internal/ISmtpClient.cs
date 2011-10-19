

namespace NLog.Internal
{
	using System;
	using System.Net;
	using System.Net.Mail;

	/// <summary>
	/// Supports mocking of SMTP Client code.
	/// </summary>
	internal interface ISmtpClient : IDisposable
	{
		string Host { get; set; }

		int Port { get; set; }

		ICredentialsByHost Credentials { get; set; }

		bool EnableSsl { get; set; }

		void Send(MailMessage msg);
	}
}
