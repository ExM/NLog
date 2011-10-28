using System;
using System.ComponentModel;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace NLog.LogReceiverService
{
	/// <summary>
	/// Log Receiver Client using WCF.
	/// </summary>
	public sealed class WcfLogReceiverClient : ClientBase<ILogReceiverClient>, ILogReceiverClient
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WcfLogReceiverClient"/> class.
		/// </summary>
		public WcfLogReceiverClient()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WcfLogReceiverClient"/> class.
		/// </summary>
		/// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
		public WcfLogReceiverClient(string endpointConfigurationName) :
			base(endpointConfigurationName)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WcfLogReceiverClient"/> class.
		/// </summary>
		/// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
		/// <param name="remoteAddress">The remote address.</param>
		public WcfLogReceiverClient(string endpointConfigurationName, string remoteAddress) :
			base(endpointConfigurationName, remoteAddress)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WcfLogReceiverClient"/> class.
		/// </summary>
		/// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
		/// <param name="remoteAddress">The remote address.</param>
		public WcfLogReceiverClient(string endpointConfigurationName, EndpointAddress remoteAddress) :
			base(endpointConfigurationName, remoteAddress)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WcfLogReceiverClient"/> class.
		/// </summary>
		/// <param name="binding">The binding.</param>
		/// <param name="remoteAddress">The remote address.</param>
		public WcfLogReceiverClient(Binding binding, EndpointAddress remoteAddress) :
			base(binding, remoteAddress)
		{
		}

		/// <summary>
		/// Occurs when the log message processing has completed.
		/// </summary>
		public event EventHandler<AsyncCompletedEventArgs> ProcessLogMessagesCompleted;

		/// <summary>
		/// Occurs when Open operation has completed.
		/// </summary>
		public event EventHandler<AsyncCompletedEventArgs> OpenCompleted;

		/// <summary>
		/// Occurs when Close operation has completed.
		/// </summary>
		public event EventHandler<AsyncCompletedEventArgs> CloseCompleted;

		/// <summary>
		/// Opens the client asynchronously.
		/// </summary>
		public void OpenAsync()
		{
			OpenAsync(null);
		}

		/// <summary>
		/// Opens the client asynchronously.
		/// </summary>
		/// <param name="userState">User-specific state.</param>
		public void OpenAsync(object userState)
		{
			InvokeAsync(OnBeginOpen, null, OnEndOpen, OnOpenCompleted, userState);
		}

		/// <summary>
		/// Closes the client asynchronously.
		/// </summary>
		public void CloseAsync()
		{
			CloseAsync(null);
		}

		/// <summary>
		/// Closes the client asynchronously.
		/// </summary>
		/// <param name="userState">User-specific state.</param>
		public void CloseAsync(object userState)
		{
			InvokeAsync(OnBeginClose, null, OnEndClose, OnCloseCompleted, userState);
		}

		/// <summary>
		/// Processes the log messages asynchronously.
		/// </summary>
		/// <param name="events">The events to send.</param>
		public void ProcessLogMessagesAsync(NLogEvents events)
		{
			ProcessLogMessagesAsync(events, null);
		}

		/// <summary>
		/// Processes the log messages asynchronously.
		/// </summary>
		/// <param name="events">The events to send.</param>
		/// <param name="userState">User-specific state.</param>
		public void ProcessLogMessagesAsync(NLogEvents events, object userState)
		{
			InvokeAsync(
				OnBeginProcessLogMessages,
				new object[] { events },
				OnEndProcessLogMessages,
				OnProcessLogMessagesCompleted,
				userState);
		}

		/// <summary>
		/// Begins processing of log messages.
		/// </summary>
		/// <param name="events">The events to send.</param>
		/// <param name="callback">The callback.</param>
		/// <param name="asyncState">Asynchronous state.</param>
		/// <returns>
		/// IAsyncResult value which can be passed to <see cref="ILogReceiverClient.EndProcessLogMessages"/>.
		/// </returns>
		IAsyncResult ILogReceiverClient.BeginProcessLogMessages(NLogEvents events, AsyncCallback callback, object asyncState)
		{
			return Channel.BeginProcessLogMessages(events, callback, asyncState);
		}

		/// <summary>
		/// Ends asynchronous processing of log messages.
		/// </summary>
		/// <param name="result">The result.</param>
		void ILogReceiverClient.EndProcessLogMessages(IAsyncResult result)
		{
			Channel.EndProcessLogMessages(result);
		}

		private IAsyncResult OnBeginProcessLogMessages(object[] inValues, AsyncCallback callback, object asyncState)
		{
			var events = (NLogEvents)inValues[0];
			return ((ILogReceiverClient)this).BeginProcessLogMessages(events, callback, asyncState);
		}

		private object[] OnEndProcessLogMessages(IAsyncResult result)
		{
			((ILogReceiverClient)this).EndProcessLogMessages(result);
			return null;
		}

		private void OnProcessLogMessagesCompleted(object state)
		{
			if (ProcessLogMessagesCompleted != null)
			{
				var e = (InvokeAsyncCompletedEventArgs)state;

				ProcessLogMessagesCompleted(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
			}
		}

		private IAsyncResult OnBeginOpen(object[] inValues, AsyncCallback callback, object asyncState)
		{
			return ((ICommunicationObject)this).BeginOpen(callback, asyncState);
		}

		private object[] OnEndOpen(IAsyncResult result)
		{
			((ICommunicationObject)this).EndOpen(result);
			return null;
		}

		private void OnOpenCompleted(object state)
		{
			if (OpenCompleted != null)
			{
				var e = (InvokeAsyncCompletedEventArgs)state;

				OpenCompleted(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
			}
		}

		private IAsyncResult OnBeginClose(object[] inValues, AsyncCallback callback, object asyncState)
		{
			return ((ICommunicationObject)this).BeginClose(callback, asyncState);
		}

		private object[] OnEndClose(IAsyncResult result)
		{
			((ICommunicationObject)this).EndClose(result);
			return null;
		}

		private void OnCloseCompleted(object state)
		{
			if (CloseCompleted != null)
			{
				var e = (InvokeAsyncCompletedEventArgs)state;

				CloseCompleted(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
			}
		}
	}
}
