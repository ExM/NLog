
namespace NLog.Internal.NetworkSenders
{
	using System.Net.Sockets;

	/// <summary>
	/// Interface for mocking socket calls.
	/// </summary>
	public interface ISocket //TODO: change namespace
	{
		bool ConnectAsync(SocketAsyncEventArgs args);

		void Close();

		bool SendAsync(SocketAsyncEventArgs args);

		bool SendToAsync(SocketAsyncEventArgs args);
	}
}
