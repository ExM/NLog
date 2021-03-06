using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using NLog.Common;
using NLog.Config;
using NLog.Internal;
using NLog.Layouts;
using NLog.LogReceiverService;

namespace NLog.Targets
{
	/// <summary>
	/// Sends log messages to a NLog Receiver Service (using WCF or Web Services).
	/// </summary>
	/// <seealso href="http://nlog-project.org/wiki/LogReceiverService_target">Documentation on NLog Wiki</seealso>
	[Target("LogReceiverService")]
	public class LogReceiverWebServiceTarget : Target
	{
		private LogEventInfoBuffer buffer = new LogEventInfoBuffer(10000, false, 10000);
		private bool inCall;

		/// <summary>
		/// Initializes a new instance of the <see cref="LogReceiverWebServiceTarget"/> class.
		/// </summary>
		public LogReceiverWebServiceTarget()
		{
			this.Parameters = new List<MethodCallParameter>();
		}

		/// <summary>
		/// Gets or sets the endpoint address.
		/// </summary>
		/// <value>The endpoint address.</value>
		/// <docgen category='Connection Options' order='10' />
		[RequiredParameter]
		public string EndpointAddress { get; set; }

		/// <summary>
		/// Gets or sets the client ID.
		/// </summary>
		/// <value>The client ID.</value>
		/// <docgen category='Payload Options' order='10' />
		public Layout ClientId { get; set; }

		/// <summary>
		/// Gets the list of parameters.
		/// </summary>
		/// <value>The parameters.</value>
		/// <docgen category='Payload Options' order='10' />
		[ArrayParameter(typeof(MethodCallParameter), "parameter")]
		public IList<MethodCallParameter> Parameters { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether to include per-event properties in the payload sent to the server.
		/// </summary>
		/// <docgen category='Payload Options' order='10' />
		public bool IncludeEventProperties { get; set; }

		/// <summary>
		/// Called when log events are being sent (test hook).
		/// </summary>
		/// <param name="events">The events.</param>
		/// <param name="asyncContinuations">The async continuations.</param>
		/// <returns>True if events should be sent, false to stop processing them.</returns>
		protected virtual bool OnSend(NLogEvents events, IEnumerable<AsyncLogEventInfo> asyncContinuations)
		{
			return true;
		}

		/// <summary>
		/// Writes logging event to the log target. Must be overridden in inheriting
		/// classes.
		/// </summary>
		/// <param name="logEvent">Logging event to be written out.</param>
		protected override void Write(AsyncLogEventInfo logEvent)
		{
			this.Write(new[] { logEvent });
		}

		/// <summary>
		/// Writes an array of logging events to the log target. By default it iterates on all
		/// events and passes them to "Append" method. Inheriting classes can use this method to
		/// optimize batch writes.
		/// </summary>
		/// <param name="logEvents">Logging events to be written out.</param>
		protected override void Write(AsyncLogEventInfo[] logEvents)
		{
			// if web service call is being processed, buffer new events and return
			// lock is being held here
			if (this.inCall)
			{
				foreach (var ev in logEvents)
				{
					this.buffer.Append(ev);
				}

				return;
			}

			var networkLogEvents = this.TranslateLogEvents(logEvents);
			this.Send(networkLogEvents, logEvents);
		}

		private static int GetStringOrdinal(NLogEvents context, Dictionary<string, int> stringTable, string value)
		{
			int stringIndex;

			if (!stringTable.TryGetValue(value, out stringIndex))
			{
				stringIndex = context.Strings.Count;
				stringTable.Add(value, stringIndex);
				context.Strings.Add(value);
			}

			return stringIndex;
		}

		private NLogEvents TranslateLogEvents(AsyncLogEventInfo[] logEvents)
		{
			string clientID = string.Empty;
			if (this.ClientId != null)
			{
				clientID = this.ClientId.Render(logEvents[0].LogEvent);
			}

			var networkLogEvents = new NLogEvents
			{
				ClientName = clientID,
				LayoutNames = new StringCollection(),
				Strings = new StringCollection(),
				BaseTimeUtc = logEvents[0].LogEvent.TimeStamp.ToUniversalTime().Ticks
			};

			var stringTable = new Dictionary<string, int>();

			for (int i = 0; i < this.Parameters.Count; ++i)
			{
				networkLogEvents.LayoutNames.Add(this.Parameters[i].Name);
			}

			if (this.IncludeEventProperties)
			{
				for (int i = 0; i < logEvents.Length; ++i)
				{
					var ev = logEvents[i].LogEvent;

					// add all event-level property names in 'LayoutNames' collection.
					foreach (var prop in ev.Properties)
					{
						string propName = prop.Key as string;
						if (propName != null)
						{
							if (!networkLogEvents.LayoutNames.Contains(propName))
							{
								networkLogEvents.LayoutNames.Add(propName);
							}
						}
					}
				}
			}

			networkLogEvents.Events = new NLogEvent[logEvents.Length];
			for (int i = 0; i < logEvents.Length; ++i)
			{
				networkLogEvents.Events[i] = this.TranslateEvent(logEvents[i].LogEvent, networkLogEvents, stringTable);
			}

			return networkLogEvents;
		}

		private void Send(NLogEvents events, IEnumerable<AsyncLogEventInfo> asyncContinuations)
		{
			if (!this.OnSend(events, asyncContinuations))
			{
				return;
			}

			var client = new SoapLogReceiverClient(this.EndpointAddress);
			this.inCall = true;
			client.BeginProcessLogMessages(
				events,
				result =>
					{
						Exception exception = null;

						try
						{
							client.EndProcessLogMessages(result);
						}
						catch (Exception ex)
						{
							if (ex.MustBeRethrown())
							{
								throw;
							}

							exception = ex;
						}

						// report error to the callers
						foreach (var ev in asyncContinuations)
						{
							ev.Continuation(exception);
						}

						// send any buffered events
						this.SendBufferedEvents();
					},
				null);
		}

		private void SendBufferedEvents()
		{
			lock (this.SyncRoot)
			{
				// clear inCall flag
				AsyncLogEventInfo[] bufferedEvents = this.buffer.GetEventsAndClear();
				if (bufferedEvents.Length > 0)
				{
					var networkLogEvents = this.TranslateLogEvents(bufferedEvents);
					this.Send(networkLogEvents, bufferedEvents);
				}
				else
				{
					// nothing in the buffer, clear in-call flag
					this.inCall = false;
				}
			}
		}

		private NLogEvent TranslateEvent(LogEventInfo eventInfo, NLogEvents context, Dictionary<string, int> stringTable)
		{
			var nlogEvent = new NLogEvent();
			nlogEvent.Id = eventInfo.SequenceID;
			nlogEvent.MessageOrdinal = GetStringOrdinal(context, stringTable, eventInfo.FormattedMessage);
			nlogEvent.LevelOrdinal = eventInfo.Level.Ordinal;
			nlogEvent.LoggerOrdinal = GetStringOrdinal(context, stringTable, eventInfo.LoggerName);
			nlogEvent.TimeDelta = eventInfo.TimeStamp.ToUniversalTime().Ticks - context.BaseTimeUtc;

			for (int i = 0; i < this.Parameters.Count; ++i)
			{
				var param = this.Parameters[i];
				var value = param.Layout.Render(eventInfo);
				int stringIndex = GetStringOrdinal(context, stringTable, value);

				nlogEvent.ValueIndexes.Add(stringIndex);
			}

			// layout names beyond Parameters.Count are per-event property names.
			for (int i = this.Parameters.Count; i < context.LayoutNames.Count; ++i)
			{
				string value;
				object propertyValue;
				
				if (eventInfo.Properties.TryGetValue(context.LayoutNames[i], out propertyValue))
				{
					value = Convert.ToString(propertyValue, CultureInfo.InvariantCulture);
				}
				else
				{
					value = string.Empty;
				}

				int stringIndex = GetStringOrdinal(context, stringTable, value);
				nlogEvent.ValueIndexes.Add(stringIndex);
			}

			return nlogEvent;
		}
	}
}
