using System.ServiceModel;

namespace NLog.LogReceiverService
{
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
