using System;
using System.ServiceModel;

namespace NLog.LogReceiverService
{

	/// <summary>
	/// Service contract for Log Receiver client.
	/// </summary>
	[ServiceContract(Namespace = LogReceiverServiceConfig.WebServiceNamespace, ConfigurationName = "NLog.LogReceiverService.ILogReceiverClient")]
	public interface ILogReceiverClient
	{
		/// <summary>
		/// Begins processing of log messages.
		/// </summary>
		/// <param name="events">The events.</param>
		/// <param name="callback">The callback.</param>
		/// <param name="asyncState">Asynchronous state.</param>
		/// <returns>
		/// IAsyncResult value which can be passed to <see cref="EndProcessLogMessages"/>.
		/// </returns>
		[OperationContractAttribute(AsyncPattern = true, Action = "http://nlog-project.org/ws/ILogReceiverServer/ProcessLogMessages", ReplyAction = "http://nlog-project.org/ws/ILogReceiverServer/ProcessLogMessagesResponse")]
		IAsyncResult BeginProcessLogMessages(NLogEvents events, AsyncCallback callback, object asyncState);

		/// <summary>
		/// Ends asynchronous processing of log messages.
		/// </summary>
		/// <param name="result">The result.</param>
		void EndProcessLogMessages(IAsyncResult result);
	}
}
