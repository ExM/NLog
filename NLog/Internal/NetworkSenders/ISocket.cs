
#if !WINDOWS_PHONE_7

namespace NLog.Internal.NetworkSenders
{
	using System.Net.Sockets;

	/// <summary>
	/// Interface for mocking socket calls.
	/// </summary>
	internal interface ISocket
	{
		bool ConnectAsync(SocketAsyncEventArgs args);

		void Close();

		bool SendAsync(SocketAsyncEventArgs args);

#if !SILVERLIGHT || (WINDOWS_PHONE && !WINDOWS_PHONE_7)
		bool SendToAsync(SocketAsyncEventArgs args);
#endif
	}
}

#endif