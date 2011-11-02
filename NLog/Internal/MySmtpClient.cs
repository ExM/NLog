using System.Net.Mail;

namespace NLog.Internal
{
	/// <summary>
	/// Supports mocking of SMTP Client code.
	/// </summary>
	public class MySmtpClient : SmtpClient, ISmtpClient
	{
	}
}
