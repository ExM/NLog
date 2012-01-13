using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace NLog.Common.NetworkSenders
{
	/// <summary>
	/// Sends messages over the network as UDP datagrams.
	/// </summary>
	public class UdpNetworkSender : NetworkSender
	{
		private object _sync = new object();
		private readonly Queue<Ticket> _pendingRequests = new Queue<Ticket>();
		private Exception _pendingError = null;
		private bool _asyncOperationInProgress = false;
		private Action<Exception> _closeContinuation = null;
		private Action<Exception> _flushContinuation = null;

		private UdpClient _client;

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
			IPEndPoint ep = ParseEndpointAddress(new Uri(Address), AddressFamily);
			_client = new UdpClient();
			_client.Connect(ep);
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
		/// Sends the specified text as a UDP datagram.
		/// </summary>
		/// <param name="bytes">The bytes to be sent.</param>
		/// <param name="offset">Offset in buffer.</param>
		/// <param name="length">Number of bytes to send.</param>
		/// <param name="asyncContinuation">The async continuation to be invoked after the buffer has been sent.</param>
		/// <remarks>To be overridden in inheriting classes.</remarks>
		protected override void DoSend(byte[] bytes, int offset, int length, Action<Exception> asyncContinuation)
		{
			var ticket = new Ticket() { Buffer = new byte[length], Continuation = asyncContinuation };
			Array.Copy(bytes, offset, ticket.Buffer, 0, length);

			lock(_sync)
				_pendingRequests.Enqueue(ticket);

			ProcessNextQueuedItem();
		}

		private void ProcessNextQueuedItem()
		{
			lock (_sync)
			{
				if(_asyncOperationInProgress)
					return;

				if (_pendingError != null)
					while(_pendingRequests.Count != 0)
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

				UdpClient clientCopy = _client;

				_client.BeginSend(t.Buffer, t.Buffer.Length,
					(ar) => SendCompleted(clientCopy, t.Continuation, ar), null);
			}
		}

		private void SendCompleted(UdpClient clientCopy, Action<Exception> continuation, IAsyncResult ar)
		{
			lock (_sync)
			{
				_asyncOperationInProgress = false;
				try
				{
					clientCopy.EndSend(ar);
				}
				catch (Exception ex)
				{
					if (ex.MustBeRethrown())
						throw;
					_pendingError = ex;
				}
			}
			
			if(continuation != null)
				continuation(_pendingError);

			ProcessNextQueuedItem();
		}

		private class Ticket
		{
			public byte[] Buffer { get; set; }
			public Action<Exception> Continuation { get; set; }
		}
	}
}
