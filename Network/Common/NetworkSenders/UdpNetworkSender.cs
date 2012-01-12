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
	internal class UdpNetworkSender : NetworkSender
	{
		private UdpClient _client;
		private IPEndPoint _endpoint;

		/// <summary>
		/// Initializes a new instance of the <see cref="UdpNetworkSender"/> class.
		/// </summary>
		/// <param name="url">URL. Must start with udp://.</param>
		/// <param name="addressFamily">The address family.</param>
		public UdpNetworkSender(string url, AddressFamily addressFamily)
			: base(url)
		{
			AddressFamily = addressFamily;
		}

		internal AddressFamily AddressFamily { get; set; }

		/// <summary>
		/// Performs sender-specific initialization.
		/// </summary>
		protected override void DoInitialize()
		{
			_endpoint = ParseEndpointAddress(new Uri(Address), AddressFamily);
			_client = new UdpClient();
			_client.Connect(_endpoint);
		}

		/// <summary>
		/// Closes the socket.
		/// </summary>
		/// <param name="continuation">The continuation.</param>
		protected override void DoClose(Action<Exception> continuation)
		{
			Exception resultEx = null;

			lock (this)
			{
				try
				{
					if (_client != null)
						_client.Close();
				}
				catch (Exception exception)
				{
					if(exception.MustBeRethrown())
						throw;
					resultEx = exception;
				}

				_client = null;
			}
			if (continuation != null)
				continuation(resultEx);
		}

		/// <summary>
		/// Sends the specified text as a UDP datagram.
		/// </summary>
		/// <param name="bytes">The bytes to be sent.</param>
		/// <param name="offset">Offset in buffer.</param>
		/// <param name="length">Number of bytes to send.</param>
		/// <param name="asyncContinuation">The async continuation to be invoked after the buffer has been sent.</param>
		/// <remarks>To be overridden in inheriting classes.</remarks>
		protected override void DoSend(byte[] bytes, int offset, int length, Action<Exception> asyncContinuation)
		{
			byte[] copy = new byte[length];
			Array.Copy(bytes, offset, copy, 0, length);

			lock (this)
			{
				UdpClient clientCopy = _client;

				Console.WriteLine("BeginSend");
				clientCopy.BeginSend(copy, length, _endpoint, (ar) =>
				{
					Console.WriteLine("EndSend");
					Exception resultEx = null;
					try
					{
						clientCopy.EndSend(ar);
					}
					catch (Exception exception)
					{
						if (exception.MustBeRethrown())
							throw;
						resultEx = exception;
					}
					if(asyncContinuation != null)
						asyncContinuation(resultEx);
				}, null);
			}
		}
	}
}
