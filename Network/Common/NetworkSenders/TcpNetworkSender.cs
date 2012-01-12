using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using NLog.Internal.NetworkSenders;
using System.Net;

namespace NLog.Common.NetworkSenders
{
	/// <summary>
	/// Sends messages over a TCP network connection.
	/// </summary>
	public class TcpNetworkSender : NetworkSender
	{
		private readonly Queue<Ticket> _pendingRequests = new Queue<Ticket>();

		private object _sync = new object();

		private TcpClient _client;
		private NetworkStream _netStream;

		private Exception _pendingError;
		private bool _asyncOperationInProgress;
		private Action<Exception> _closeContinuation;
		private Action<Exception> _flushContinuation;

		/// <summary>
		/// Initializes a new instance of the <see cref="TcpNetworkSender"/> class.
		/// </summary>
		/// <param name="url">URL. Must start with tcp://.</param>
		/// <param name="addressFamily">The address family.</param>
		public TcpNetworkSender(string url, AddressFamily addressFamily)
			: base(url)
		{
			AddressFamily = addressFamily;
		}

		internal AddressFamily AddressFamily { get; set; }

		/// <summary>
		/// Creates the socket with given parameters. 
		/// </summary>
		/// <param name="addressFamily">The address family.</param>
		/// <param name="socketType">Type of the socket.</param>
		/// <param name="protocolType">Type of the protocol.</param>
		/// <returns>Instance of <see cref="ISocket" /> which represents the socket.</returns>
		public virtual ISocket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
		{
			return new SocketProxy(addressFamily, socketType, protocolType);
		}

		/// <summary>
		/// Performs sender-specific initialization.
		/// </summary>
		protected override void DoInitialize()
		{
			IPEndPoint ep = ParseEndpointAddress(new Uri(Address), AddressFamily);
			_client = new TcpClient(ep.AddressFamily);
			lock (_sync)
				_asyncOperationInProgress = true;
			_client.BeginConnect(ep.Address, ep.Port, ConnectCompleted, null);
		}

		private void ConnectCompleted(IAsyncResult ar)
		{
			lock (_sync)
			{
				_asyncOperationInProgress = false;
				try
				{
					_client.EndConnect(ar);
					_netStream = _client.GetStream();
				}
				catch (Exception ex)
				{
					if (ex.MustBeRethrown())
						throw;
					_pendingError = ex;
				}
			}

			ProcessNextQueuedItem();
		}

		/// <summary>
		/// Closes the socket.
		/// </summary>
		/// <param name="continuation">The continuation.</param>
		protected override void DoClose(Action<Exception> continuation)
		{
			lock (_sync)
			{
				if (_asyncOperationInProgress)
					_closeContinuation += continuation;
				else
					CloseSocket(continuation);
			}
		}

		private void CloseSocket(Action<Exception> continuation)
		{
			Exception resultEx = null;

			lock (_sync)
			{
				try
				{
					if (_client != null)
						_client.Close();
				}
				catch (Exception exception)
				{
					if (exception.MustBeRethrown())
						throw;
					resultEx = exception;
				}

				_client = null;
			}
			if (continuation != null)
				continuation(resultEx);
		}

		/// <summary>
		/// Performs sender-specific flush.
		/// </summary>
		/// <param name="continuation">The continuation.</param>
		protected override void DoFlush(Action<Exception> continuation)
		{
			lock (_sync)
			{
				if (!_asyncOperationInProgress && _pendingRequests.Count == 0)
					continuation(null);
				else
					_flushContinuation += continuation;
			}
		}

		/// <summary>
		/// Sends the specified text over the connected socket.
		/// </summary>
		/// <param name="bytes">The bytes to be sent.</param>
		/// <param name="offset">Offset in buffer.</param>
		/// <param name="length">Number of bytes to send.</param>
		/// <param name="asyncContinuation">The async continuation to be invoked after the buffer has been sent.</param>
		/// <remarks>To be overridden in inheriting classes.</remarks>
		protected override void DoSend(byte[] bytes, int offset, int length, Action<Exception> asyncContinuation)
		{
			var ticket = new Ticket() { Buffer = bytes, Offset = offset, Length = length, Continuation = asyncContinuation };

			lock (_sync)
				_pendingRequests.Enqueue(ticket);

			ProcessNextQueuedItem();
		}

		private void WriteCompleted(Action<Exception> continuation, IAsyncResult ar)
		{
			lock (_sync)
			{
				_asyncOperationInProgress = false;
				try
				{
					_netStream.EndWrite(ar);
				}
				catch (Exception ex)
				{
					if (ex.MustBeRethrown())
						throw;
					_pendingError = ex;
				}
			}

			if (continuation != null)
				continuation(_pendingError);

			ProcessNextQueuedItem();
		}

		private void ProcessNextQueuedItem()
		{
			lock (_sync)
			{
				if (_asyncOperationInProgress)
					return;

				if (_pendingError != null)
					while (_pendingRequests.Count != 0)
						_pendingRequests.Dequeue().Continuation(_pendingError);

				if (_pendingRequests.Count == 0)
				{
					var fc = _flushContinuation;
					if (fc != null)
					{
						_flushContinuation = null;
						fc(_pendingError);
					}

					var cc = _closeContinuation;
					if (cc != null)
					{
						_closeContinuation = null;
						CloseSocket(cc);
					}

					return;
				}

				var t = _pendingRequests.Dequeue();

				_asyncOperationInProgress = true;
				_netStream.BeginWrite(t.Buffer, t.Offset, t.Length,
					(ar) => WriteCompleted(t.Continuation, ar), null);
			}
		}

		private class Ticket
		{
			public byte[] Buffer { get; set; }
			public int Offset { get; set; }
			public int Length { get; set; }
			public Action<Exception> Continuation { get; set; }
		}
	}
}
