using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NUnit.Framework;
using NLog.Common.NetworkSenders;

namespace NLog.UnitTests.Common.NetworkSenders
{
	[TestFixture]
	public class NetworkSenderTests : NLogTestBase
	{
		[Test]
		public void CreateTcpSender()
		{
			var s = NetworkSender.CreateDefaultSender("tcp://hostname:123");
			Assert.IsInstanceOf<TcpNetworkSender>(s);
		}
		
		[Test]
		public void CreateUdpSender()
		{
			var s = NetworkSender.CreateDefaultSender("udp://hostname:123");
			Assert.IsInstanceOf<UdpNetworkSender>(s);
		}
		
		[Test]
		public void CreateHttpSender()
		{
			var s = NetworkSender.CreateDefaultSender("http://hostname:123");
			Assert.IsInstanceOf<HttpNetworkSender>(s);
		}
	}
}
