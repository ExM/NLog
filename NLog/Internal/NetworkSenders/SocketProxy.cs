
#if !WINDOWS_PHONE_7

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

#if USE_LEGACY_ASYNC_API || NET_CF
		// emulate missing .NET CF behavior

		/// <summary>
		/// Invokes ConnectAsync method on the wrapped socket.
		/// </summary>
		/// <param name="args">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
		/// <returns>Result of original method.</returns>
		public bool ConnectAsync(SocketAsyncEventArgs args)
		{
			this.socket.BeginConnect(args.RemoteEndPoint, args.EndConnect, this.socket);
			return true;
		}

		/// <summary>
		/// Invokes SendAsync method on the wrapped socket.
		/// </summary>
		/// <param name="args">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
		/// <returns>Result of original method.</returns>
		public bool SendAsync(SocketAsyncEventArgs args)
		{
			this.socket.BeginSend(args.Buffer, args.Offset, args.Count, args.SocketFlags, args.EndSend, this.socket);
			return true;
		}

		/// <summary>
		/// Invokes SendToAsync method on the wrapped socket.
		/// </summary>
		/// <param name="args">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
		/// <returns>Result of original method.</returns>
		public bool SendToAsync(SocketAsyncEventArgs args)
		{
			this.socket.BeginSendTo(args.Buffer, args.Offset, args.Count, args.SocketFlags, args.RemoteEndPoint, args.EndSendTo, this.socket);
			return true;
		}
#else
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

#if !SILVERLIGHT || (WINDOWS_PHONE && !WINDOWS_PHONE_7)
		/// <summary>
		/// Invokes SendToAsync method on the wrapped socket.
		/// </summary>
		/// <param name="args">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
		/// <returns>Result of original method.</returns>
		public bool SendToAsync(SocketAsyncEventArgs args)
		{
			return this.socket.SendToAsync(args);
		}
#endif

#endif

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			((IDisposable)this.socket).Dispose();
		}
	}
}

#endif