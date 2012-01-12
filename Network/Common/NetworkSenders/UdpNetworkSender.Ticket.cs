using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using NLog.Internal.NetworkSenders;

namespace NLog.Common.NetworkSenders
{
	/// <summary>
	/// Sends messages over the network as UDP datagrams.
	/// </summary>
	internal partial class UdpNetworkSender : NetworkSender
	{
		private class Ticket
		{
			public byte[] Buffer {get; set;}
			public Action<Exception> Continuation {get; set;}
		}
	}
}
