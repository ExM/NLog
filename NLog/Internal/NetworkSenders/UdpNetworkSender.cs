
namespace NLog.Internal.NetworkSenders
{
	using System;
	using System.IO;
	using System.Net;
	using System.Net.Sockets;
	using NLog.Common;

	/// <summary>
	/// Sends messages over the network as UDP datagrams.
	/// </summary>
	internal class UdpNetworkSender : NetworkSender
	{
		private ISocket socket;
		private EndPoint endpoint;

		/// <summary>
		/// Initializes a new instance of the <see cref="UdpNetworkSender"/> class.
		/// </summary>
		/// <param name="url">URL. Must start with udp://.</param>
		/// <param name="addressFamily">The address family.</param>
		public UdpNetworkSender(string url, AddressFamily addressFamily)
			: base(url)
		{
			this.AddressFamily = addressFamily;
		}

		internal AddressFamily AddressFamily { get; set; }

		/// <summary>
		/// Creates the socket.
		/// </summary>
		/// <param name="addressFamily">The address family.</param>
		/// <param name="socketType">Type of the socket.</param>
		/// <param name="protocolType">Type of the protocol.</param>
		/// <returns>Implementation of <see cref="ISocket"/> to use.</returns>
		protected internal virtual ISocket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
		{
			return new SocketProxy(addressFamily, socketType, protocolType);
		}

		/// <summary>
		/// Performs sender-specific initialization.
		/// </summary>
		protected override void DoInitialize()
		{
			this.endpoint = this.ParseEndpointAddress(new Uri(this.Address), this.AddressFamily);
			this.socket = this.CreateSocket(this.endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
		}

		/// <summary>
		/// Closes the socket.
		/// </summary>
		/// <param name="continuation">The continuation.</param>
		protected override void DoClose(AsyncContinuation continuation)
		{
			lock (this)
			{
				try
				{
					if (this.socket != null)
					{
						this.socket.Close();
					}
				}
				catch (Exception exception)
				{
					if (exception.MustBeRethrown())
					{
						throw;
					}
				}

				this.socket = null;
			}
		}

		/// <summary>
		/// Sends the specified text as a UDP datagram.
		/// </summary>
		/// <param name="bytes">The bytes to be sent.</param>
		/// <param name="offset">Offset in buffer.</param>
		/// <param name="length">Number of bytes to send.</param>
		/// <param name="asyncContinuation">The async continuation to be invoked after the buffer has been sent.</param>
		/// <remarks>To be overridden in inheriting classes.</remarks>
		protected override void DoSend(byte[] bytes, int offset, int length, AsyncContinuation asyncContinuation)
		{
			lock (this)
			{
				var args = new SocketAsyncEventArgs();
				args.SetBuffer(bytes, offset, length);
				args.UserToken = asyncContinuation;
				args.Completed += this.SocketOperationCompleted;
				args.RemoteEndPoint = this.endpoint;

				if (!this.socket.SendToAsync(args))
				{
					this.SocketOperationCompleted(this.socket, args);
				}
			}
		}

		private void SocketOperationCompleted(object sender, SocketAsyncEventArgs e)
		{
			var asyncContinuation = e.UserToken as AsyncContinuation;

			Exception error = null;

			if (e.SocketError != SocketError.Success)
			{
				error = new IOException("Error: " + e.SocketError);
			}

			e.Dispose();

			if (asyncContinuation != null)
			{
				asyncContinuation(error);
			}
		}
	}
}
