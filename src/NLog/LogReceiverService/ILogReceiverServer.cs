
#if WCF_SUPPORTED

namespace NLog.LogReceiverService
{
	using System.ServiceModel;

	/// <summary>
	/// Service contract for Log Receiver server.
	/// </summary>
	[ServiceContract(Namespace = LogReceiverServiceConfig.WebServiceNamespace)]
	public interface ILogReceiverServer
	{
		/// <summary>
		/// Processes the log messages.
		/// </summary>
		/// <param name="events">The events.</param>
		[OperationContract]
		void ProcessLogMessages(NLogEvents events);
	}
}

#endif