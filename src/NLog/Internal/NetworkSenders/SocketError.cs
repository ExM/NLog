
#if NET_CF || USE_LEGACY_ASYNC_API

namespace NLog.Internal.NetworkSenders
{
	/// <summary>
	/// Emulate missing functionality from .NET Compact Framework
	/// </summary>
	internal enum SocketError
	{
		Success,

		SocketError,
	}
}

#endif