using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using NUnit.Framework;
using NLog.Internal;
using NLog.Layouts;
using NLog.Targets;
using NLog.Common;

namespace NLog.UnitTests.Targets
{
	[TestFixture]
	public class MailTargetTests : NLogTestBase
	{
		[Test]
		public void SimpleEmailTest()
		{
			var mmt = new MockMailTarget
			{
				From = "foo@bar.com",
				To = "bar@foo.com",
				CC = "me@myserver.com;you@yourserver.com",
				Bcc = "foo@myserver.com;bar@yourserver.com",
				Subject = "Hello from NLog",
				SmtpServer = "server1",
				SmtpPort = 27,
				Body = "${level} ${logger} ${message}"
			};

			mmt.Initialize(CommonCfg);

			var exceptions = new List<Exception>();
			mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(exceptions.Add));
			Assert.IsNull(exceptions[0], Convert.ToString(exceptions[0]));

			Assert.AreEqual(1, mmt.CreatedMocks.Count);

			var mock = mmt.CreatedMocks[0];
			Assert.AreEqual(1, mock.MessagesSent.Count);
			Assert.AreEqual("server1", mock.Host);
			Assert.AreEqual(27, mock.Port);
			Assert.IsFalse(mock.EnableSsl);
			Assert.IsNull(mock.Credentials);

			var msg = mock.MessagesSent[0];
			Assert.AreEqual("Hello from NLog", msg.Subject);
			Assert.AreEqual("foo@bar.com", msg.From.Address);
			Assert.AreEqual(1, msg.To.Count);
			Assert.AreEqual("bar@foo.com", msg.To[0].Address);
			Assert.AreEqual(2, msg.CC.Count);
			Assert.AreEqual("me@myserver.com", msg.CC[0].Address);
			Assert.AreEqual("you@yourserver.com", msg.CC[1].Address);
			Assert.AreEqual(2, msg.Bcc.Count);
			Assert.AreEqual("foo@myserver.com", msg.Bcc[0].Address);
			Assert.AreEqual("bar@yourserver.com", msg.Bcc[1].Address);
			Assert.AreEqual(msg.Body, "Info MyLogger log message 1");
		}

		[Test]
		public void NtlmEmailTest()
		{
			var mmt = new MockMailTarget
			{
				From = "foo@bar.com",
				To = "bar@foo.com",
				SmtpServer = "server1",
				SmtpAuthentication = SmtpAuthenticationMode.Ntlm,
			};

			mmt.Initialize(CommonCfg);

			var exceptions = new List<Exception>();
			mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(exceptions.Add));
			Assert.IsNull(exceptions[0], Convert.ToString(exceptions[0]));

			Assert.AreEqual(1, mmt.CreatedMocks.Count);

			var mock = mmt.CreatedMocks[0];
			Assert.AreEqual(CredentialCache.DefaultNetworkCredentials, mock.Credentials);
		}

		[Test]
		public void BasicAuthEmailTest()
		{
			try
			{
				var mmt = new MockMailTarget
				{
					From = "foo@bar.com",
					To = "bar@foo.com",
					SmtpServer = "server1",
					SmtpAuthentication = SmtpAuthenticationMode.Basic,
					SmtpUserName = "${mdc:username}",
					SmtpPassword = "${mdc:password}",
				};

				mmt.Initialize(CommonCfg);

				var exceptions = new List<Exception>();
				MappedDiagnosticsContext.Set("username", "u1");
				MappedDiagnosticsContext.Set("password", "p1");
				mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(exceptions.Add));
				Assert.IsNull(exceptions[0], Convert.ToString(exceptions[0]));

				Assert.AreEqual(1, mmt.CreatedMocks.Count);

				var mock = mmt.CreatedMocks[0];
				var credential = mock.Credentials as NetworkCredential;
				Assert.IsNotNull(credential);
				Assert.AreEqual("u1", credential.UserName);
				Assert.AreEqual("p1", credential.Password);
				Assert.AreEqual(string.Empty, credential.Domain);
			}
			finally
			{
				MappedDiagnosticsContext.Clear();
			}
		}

		[Test]
		public void CsvLayoutTest()
		{
			var layout = new CsvLayout()
			{
				Delimiter = CsvColumnDelimiterMode.Semicolon,
				WithHeader = true,
				Columns =
				{
					new CsvColumn("name", "${logger}"),
					new CsvColumn("level", "${level}"),
					new CsvColumn("message", "${message}"),
				}
			};

			var mmt = new MockMailTarget
			{
				From = "foo@bar.com",
				To = "bar@foo.com",
				SmtpServer = "server1",
				AddNewLines = true,
				Layout = layout,
			};

			layout.Initialize(CommonCfg);

			mmt.Initialize(CommonCfg);

			var exceptions = new List<Exception>();
			mmt.WriteAsyncLogEvents(
				new LogEventInfo(LogLevel.Info, "MyLogger1", "log message 1").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Debug, "MyLogger2", "log message 2").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Error, "MyLogger3", "log message 3").WithContinuation(exceptions.Add));
			Assert.IsNull(exceptions[0], Convert.ToString(exceptions[0]));

			Assert.AreEqual(1, mmt.CreatedMocks.Count);

			var mock = mmt.CreatedMocks[0];
			Assert.AreEqual(1, mock.MessagesSent.Count);
			var msg = mock.MessagesSent[0];
			string expectedBody = "name;level;message\nMyLogger1;Info;log message 1\nMyLogger2;Debug;log message 2\nMyLogger3;Error;log message 3\n";
			Assert.AreEqual(expectedBody, msg.Body);
		}

		[Test]
		public void PerMessageServer()
		{
			var mmt = new MockMailTarget
			{
				From = "foo@bar.com",
				To = "bar@foo.com",
				SmtpServer = "${logger}.mydomain.com",
				Body = "${message}",
				AddNewLines = true,
			};

			mmt.Initialize(CommonCfg);

			var exceptions = new List<Exception>();
			mmt.WriteAsyncLogEvents(
				new LogEventInfo(LogLevel.Info, "MyLogger1", "log message 1").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Debug, "MyLogger2", "log message 2").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Error, "MyLogger1", "log message 3").WithContinuation(exceptions.Add));
			Assert.IsNull(exceptions[0], Convert.ToString(exceptions[0]));

			// 2 messages are sent, one using MyLogger1.mydomain.com, another using MyLogger2.mydomain.com
			Assert.AreEqual(2, mmt.CreatedMocks.Count);

			var mock1 = mmt.CreatedMocks[0];
			Assert.AreEqual("MyLogger1.mydomain.com", mock1.Host);
			Assert.AreEqual(1, mock1.MessagesSent.Count);
			
			var msg1 = mock1.MessagesSent[0];
			Assert.AreEqual("log message 1\nlog message 3\n", msg1.Body);

			var mock2 = mmt.CreatedMocks[1];
			Assert.AreEqual("MyLogger2.mydomain.com", mock2.Host);
			Assert.AreEqual(1, mock2.MessagesSent.Count);

			var msg2 = mock2.MessagesSent[0];
			Assert.AreEqual("log message 2\n", msg2.Body);
		}

		[Test]
		public void ErrorHandlingTest()
		{
			var mmt = new MockMailTarget
			{
				From = "foo@bar.com",
				To = "bar@foo.com",
				SmtpServer = "${logger}",
				Body = "${message}",
				AddNewLines = true,
			};

			mmt.Initialize(CommonCfg);

			var exceptions = new List<Exception>();
			var exceptions2 = new List<Exception>();

			mmt.WriteAsyncLogEvents(
				new LogEventInfo(LogLevel.Info, "MyLogger1", "log message 1").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Debug, "ERROR", "log message 2").WithContinuation(exceptions2.Add),
				new LogEventInfo(LogLevel.Error, "MyLogger1", "log message 3").WithContinuation(exceptions.Add));
			Assert.IsNull(exceptions[0], Convert.ToString(exceptions[0]));
			Assert.IsNull(exceptions[1], Convert.ToString(exceptions[1]));

			Assert.IsNotNull(exceptions2[0]);
			Assert.AreEqual("Some SMTP error.", exceptions2[0].Message);

			// 2 messages are sent, one using MyLogger1.mydomain.com, another using MyLogger2.mydomain.com
			Assert.AreEqual(2, mmt.CreatedMocks.Count);

			var mock1 = mmt.CreatedMocks[0];
			Assert.AreEqual("MyLogger1", mock1.Host);
			Assert.AreEqual(1, mock1.MessagesSent.Count);

			var msg1 = mock1.MessagesSent[0];
			Assert.AreEqual("log message 1\nlog message 3\n", msg1.Body);

			var mock2 = mmt.CreatedMocks[1];
			Assert.AreEqual("ERROR", mock2.Host);
			Assert.AreEqual(1, mock2.MessagesSent.Count);

			var msg2 = mock2.MessagesSent[0];
			Assert.AreEqual("log message 2\n", msg2.Body);
		}

		/// <summary>
		/// Tests that it is possible to user different email address for each log message,
		/// for example by using ${logger}, ${event-context} or any other layout renderer.
		/// </summary>
		[Test]
		public void PerMessageAddress()
		{
			var mmt = new MockMailTarget
			{
				From = "foo@bar.com",
				To = "${logger}@foo.com",
				Body = "${message}",
				SmtpServer = "server1.mydomain.com",
				AddNewLines = true,
			};

			mmt.Initialize(CommonCfg);

			var exceptions = new List<Exception>();
			mmt.WriteAsyncLogEvents(
				new LogEventInfo(LogLevel.Info, "MyLogger1", "log message 1").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Debug, "MyLogger2", "log message 2").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Error, "MyLogger1", "log message 3").WithContinuation(exceptions.Add));
			Assert.IsNull(exceptions[0], Convert.ToString(exceptions[0]));

			// 2 messages are sent, one using MyLogger1.mydomain.com, another using MyLogger2.mydomain.com
			Assert.AreEqual(2, mmt.CreatedMocks.Count);

			var mock1 = mmt.CreatedMocks[0];
			Assert.AreEqual(1, mock1.MessagesSent.Count);

			var msg1 = mock1.MessagesSent[0];
			Assert.AreEqual("MyLogger1@foo.com", msg1.To[0].Address);
			Assert.AreEqual("log message 1\nlog message 3\n", msg1.Body);

			var mock2 = mmt.CreatedMocks[1];
			Assert.AreEqual(1, mock2.MessagesSent.Count);

			var msg2 = mock2.MessagesSent[0];
			Assert.AreEqual("MyLogger2@foo.com", msg2.To[0].Address);
			Assert.AreEqual("log message 2\n", msg2.Body);
		}

		[Test]
		public void CustomHeaderAndFooter()
		{
			var mmt = new MockMailTarget
			{
				From = "foo@bar.com",
				To = "bar@foo.com",
				SmtpServer = "server1",
				AddNewLines = true,
				Layout = "${message}",
				Header = "First event: ${logger}",
				Footer = "Last event: ${logger}",
			};

			mmt.Initialize(CommonCfg);

			var exceptions = new List<Exception>();
			mmt.WriteAsyncLogEvents(
				new LogEventInfo(LogLevel.Info, "MyLogger1", "log message 1").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Debug, "MyLogger2", "log message 2").WithContinuation(exceptions.Add),
				new LogEventInfo(LogLevel.Error, "MyLogger3", "log message 3").WithContinuation(exceptions.Add));
			Assert.IsNull(exceptions[0], Convert.ToString(exceptions[0]));

			Assert.AreEqual(1, mmt.CreatedMocks.Count);

			var mock = mmt.CreatedMocks[0];
			Assert.AreEqual(1, mock.MessagesSent.Count);
			var msg = mock.MessagesSent[0];
			string expectedBody = "First event: MyLogger1\nlog message 1\nlog message 2\nlog message 3\nLast event: MyLogger3\n";
			Assert.AreEqual(expectedBody, msg.Body);
		}

		[Test]
		public void DefaultSmtpClientTest()
		{
			var mailTarget = new MailTarget();
			var client = mailTarget.CreateSmtpClient();
			Assert.IsInstanceOf<MySmtpClient>(client);
		}

		public class MockSmtpClient : ISmtpClient
		{
			public MockSmtpClient()
			{
				this.MessagesSent = new List<MailMessage>();
			}

			public string Host { get; set; }
			public int Port { get; set; }
			public ICredentialsByHost Credentials { get; set; }
			public bool EnableSsl { get; set; }
			public List<MailMessage> MessagesSent { get; private set; }

			public void Send(MailMessage msg)
			{
				this.MessagesSent.Add(msg);
				if (Host == "ERROR")
				{
					throw new InvalidOperationException("Some SMTP error.");
				}
			}

			public void Dispose()
			{
			}
		}

		public class MockMailTarget : MailTarget
		{
			public List<MockSmtpClient> CreatedMocks = new List<MockSmtpClient>();

			public override ISmtpClient CreateSmtpClient()
			{
				var mock = new MockSmtpClient();
				CreatedMocks.Add(mock);
				return mock;
			}
		}
	}
}
