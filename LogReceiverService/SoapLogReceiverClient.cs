using System;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace NLog.LogReceiverService
{
	/// <summary>
	/// Log Receiver Client using legacy SOAP client.
	/// </summary>
	[WebServiceBindingAttribute(Name = "BasicHttpBinding_ILogReceiverServer", Namespace = "http://tempuri.org/")]
	public class SoapLogReceiverClient : SoapHttpClientProtocol, ILogReceiverClient
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SoapLogReceiverClient"/> class.
		/// </summary>
		/// <param name="url">The service URL.</param>
		public SoapLogReceiverClient(string url)
		{
			Url = url;
		}

		/// <summary>
		/// Processes the log messages.
		/// </summary>
		/// <param name="events">The events.</param>
		[SoapDocumentMethodAttribute("http://nlog-project.org/ws/ILogReceiverServer/ProcessLogMessages", 
			RequestNamespace = LogReceiverServiceConfig.WebServiceNamespace, 
			ResponseNamespace = LogReceiverServiceConfig.WebServiceNamespace, 
			Use = SoapBindingUse.Literal, 
			ParameterStyle = SoapParameterStyle.Wrapped)]
		public void ProcessLogMessages(NLogEvents events)
		{
			Invoke("ProcessLogMessages", new object[] { events });
		}

		/// <summary>
		/// Begins processing of log messages.
		/// </summary>
		/// <param name="events">The events.</param>
		/// <param name="callback">The callback.</param>
		/// <param name="asyncState">Asynchronous state.</param>
		/// <returns>
		/// IAsyncResult value which can be passed to <see cref="ILogReceiverClient.EndProcessLogMessages"/>.
		/// </returns>
		public IAsyncResult BeginProcessLogMessages(NLogEvents events, AsyncCallback callback, object asyncState)
		{
			return BeginInvoke("ProcessLogMessages", new object[] { events }, callback, asyncState);
		}

		/// <summary>
		/// Ends asynchronous processing of log messages.
		/// </summary>
		/// <param name="result">The result.</param>
		public void EndProcessLogMessages(IAsyncResult result)
		{
			EndInvoke(result);
		}
	}
}
