using System.Net.Sockets;

namespace NLog.Common.NetworkSenders
{
	/// <summary>
	/// Interface for mocking socket calls.
	/// </summary>
	public interface ISocket
	{
		bool ConnectAsync(SocketAsyncEventArgs args);

		void Close();

		bool SendAsync(SocketAsyncEventArgs args);

		bool SendToAsync(SocketAsyncEventArgs args);
	}
}
