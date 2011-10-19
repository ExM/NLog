

namespace NLog.Internal.NetworkSenders
{
	using System;
	using System.Net.Sockets;
	using System.Security;

	/// <summary>
	/// Socket proxy for mocking Socket code.
	/// </summary>
	internal sealed class SocketProxy : ISocket, IDisposable
	{
		private readonly Socket socket;

		/// <summary>
		/// Initializes a new instance of the <see cref="SocketProxy"/> class.
		/// </summary>
		/// <param name="addressFamily">The address family.</param>
		/// <param name="socketType">Type of the socket.</param>
		/// <param name="protocolType">Type of the protocol.</param>
		internal SocketProxy(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
		{
			this.socket = new Socket(addressFamily, socketType, protocolType);
		}

		/// <summary>
		/// Closes the wrapped socket.
		/// </summary>
		public void Close()
		{
			this.socket.Close();
		}

		/// <summary>
		/// Invokes ConnectAsync method on the wrapped socket.
		/// </summary>
		/// <param name="args">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
		/// <returns>Result of original method.</returns>
		public bool ConnectAsync(SocketAsyncEventArgs args)
		{
			return this.socket.ConnectAsync(args);
		}

		/// <summary>
		/// Invokes SendAsync method on the wrapped socket.
		/// </summary>
		/// <param name="args">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
		/// <returns>Result of original method.</returns>
		public bool SendAsync(SocketAsyncEventArgs args)
		{
			return this.socket.SendAsync(args);
		}

		/// <summary>
		/// Invokes SendToAsync method on the wrapped socket.
		/// </summary>
		/// <param name="args">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
		/// <returns>Result of original method.</returns>
		public bool SendToAsync(SocketAsyncEventArgs args)
		{
			return this.socket.SendToAsync(args);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			((IDisposable)this.socket).Dispose();
		}
	}
}
