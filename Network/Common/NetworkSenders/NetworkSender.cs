using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NLog.Common.NetworkSenders
{
	/// <summary>
	/// A base class for all network senders. Supports one-way sending of messages
	/// over various protocols.
	/// </summary>
	public abstract class NetworkSender : IDisposable
	{
		private static int currentSendTime;

		/// <summary>
		/// Initializes a new instance of the <see cref="NetworkSender" /> class.
		/// </summary>
		/// <param name="url">The network URL.</param>
		protected NetworkSender(string url)
		{
			this.Address = url;
			this.LastSendTime = Interlocked.Increment(ref currentSendTime);
		}

		/// <summary>
		/// Finalizes an instance of the NetworkSender class.
		/// </summary>
		~NetworkSender()
		{
			this.Dispose(false);
		}

		/// <summary>
		/// Gets the address of the network endpoint.
		/// </summary>
		public string Address { get; private set; }

		/// <summary>
		/// Gets the last send time.
		/// </summary>
		public int LastSendTime { get; private set; }

		/// <summary>
		/// Initializes this network sender.
		/// </summary>
		public void Initialize()
		{
			this.DoInitialize();
		}

		/// <summary>
		/// Closes the sender and releases any unmanaged resources.
		/// </summary>
		/// <param name="continuation">The continuation.</param>
		public void Close(Action<Exception> continuation)
		{
			this.DoClose(continuation);
		}

		/// <summary>
		/// Flushes any pending messages and invokes a continuation.
		/// </summary>
		/// <param name="continuation">The continuation.</param>
		public void FlushAsync(Action<Exception> continuation)
		{
			this.DoFlush(continuation);
		}

		/// <summary>
		/// Send the given text over the specified protocol.
		/// </summary>
		/// <param name="bytes">Bytes to be sent.</param>
		/// <param name="offset">Offset in buffer.</param>
		/// <param name="length">Number of bytes to send.</param>
		/// <param name="asyncContinuation">The asynchronous continuation.</param>
		public void Send(byte[] bytes, int offset, int length, Action<Exception> asyncContinuation)
		{
			this.LastSendTime = Interlocked.Increment(ref currentSendTime);
			this.DoSend(bytes, offset, length, asyncContinuation);
		}

		/// <summary>
		/// Closes the sender and releases any unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Performs sender-specific initialization.
		/// </summary>
		protected virtual void DoInitialize()
		{
		}

		/// <summary>
		/// Performs sender-specific close operation.
		/// </summary>
		/// <param name="continuation">The continuation.</param>
		protected virtual void DoClose(Action<Exception> continuation)
		{
			continuation(null);
		}

		/// <summary>
		/// Performs sender-specific flush.
		/// </summary>
		/// <param name="continuation">The continuation.</param>
		protected virtual void DoFlush(Action<Exception> continuation)
		{
			continuation(null);
		}

		/// <summary>
		/// Actually sends the given text over the specified protocol.
		/// </summary>
		/// <param name="bytes">The bytes to be sent.</param>
		/// <param name="offset">Offset in buffer.</param>
		/// <param name="length">Number of bytes to send.</param>
		/// <param name="asyncContinuation">The async continuation to be invoked after the buffer has been sent.</param>
		/// <remarks>To be overridden in inheriting classes.</remarks>
		protected abstract void DoSend(byte[] bytes, int offset, int length, Action<Exception> asyncContinuation);

		/// <summary>
		/// Parses the URI into an endpoint address.
		/// </summary>
		/// <param name="uri">The URI to parse.</param>
		/// <param name="addressFamily">The address family.</param>
		/// <returns>Parsed endpoint.</returns>
		protected virtual EndPoint ParseEndpointAddress(Uri uri, AddressFamily addressFamily)
		{
			switch (uri.HostNameType)
			{
				case UriHostNameType.IPv4:
				case UriHostNameType.IPv6:
					return new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port);

				default:
					{
						var addresses = Dns.GetHostEntry(uri.Host).AddressList;
						foreach (var addr in addresses)
						{
							if (addr.AddressFamily == addressFamily || addressFamily == AddressFamily.Unspecified)
							{
								return new IPEndPoint(addr, uri.Port);
							}
						}

						throw new IOException("Cannot resolve '" + uri.Host + "' to an address in '" + addressFamily + "'");
					}
			}
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.Close(ex => { });
			}
		}

		/// <summary>
		/// Creates a new instance of the network sender based on a network URL:.
		/// </summary>
		/// <param name="url">
		/// URL that determines the network sender to be created.
		/// </param>
		/// <returns>
		/// A newly created network sender.
		/// </returns>
		public static NetworkSender CreateDefaultSender(string url)
		{
			if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
				return new HttpNetworkSender(url);

			if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
				return new HttpNetworkSender(url);

			if (url.StartsWith("tcp://", StringComparison.OrdinalIgnoreCase))
				return new TcpNetworkSender(url, AddressFamily.Unspecified);

			if (url.StartsWith("tcp4://", StringComparison.OrdinalIgnoreCase))
				return new TcpNetworkSender(url, AddressFamily.InterNetwork);

			if (url.StartsWith("tcp6://", StringComparison.OrdinalIgnoreCase))
				return new TcpNetworkSender(url, AddressFamily.InterNetworkV6);

			if (url.StartsWith("udp://", StringComparison.OrdinalIgnoreCase))
				return new UdpNetworkSender(url, AddressFamily.Unspecified);

			if (url.StartsWith("udp4://", StringComparison.OrdinalIgnoreCase))
				return new UdpNetworkSender(url, AddressFamily.InterNetwork);

			if (url.StartsWith("udp6://", StringComparison.OrdinalIgnoreCase))
				return new UdpNetworkSender(url, AddressFamily.InterNetworkV6);

			throw new ArgumentException("Unrecognized network address", "url");
		}
	}
}
