using System;
using System.Text;
using NLog;

namespace InheritFromLogger
{
	/// <summary>
	/// Provides methods to write messages with event IDs - useful for the Event Log target
	/// Inherits from the Logger class.
	/// </summary>
	public class LoggerWithEventID : Logger
	{
		public LoggerWithEventID()
		{
		}

		// additional method that takes eventID as an argument
		public void DebugWithEventID(int eventID, string message, params object[] args)
		{
			if (IsDebugEnabled)
			{
				// create log event 
				LogEventInfo lei = new LogEventInfo(LogLevel.Debug, Name, null, message, args);

				// set the per-log context data
				// this data can be retrieved using ${event-context:EventID}
				lei.Properties["EventID"] = eventID;

				// log the message
				base.Log(typeof(LoggerWithEventID), lei);
			}
		}

		// other methods omitted for brevity
	}

	class Program
	{
		// get the current class logger as an instance of LoggerWithEventID class

		private static LoggerWithEventID LoggerWithEventID = (LoggerWithEventID)LogManager.GetCurrentClassLogger(typeof(LoggerWithEventID));

		static void Main(string[] args)
		{
			// this writes 5 messages to the Event Log, each with a different EventID

			LoggerWithEventID.DebugWithEventID(123, "message 1", 1, 2, 3);
			LoggerWithEventID.DebugWithEventID(124, "message 2", 1, 2, 3);
			LoggerWithEventID.DebugWithEventID(125, "message 3", 1, 2, 3);
			LoggerWithEventID.DebugWithEventID(126, "message 4", 1, 2, 3);
			LoggerWithEventID.DebugWithEventID(127, "message 5", 1, 2, 3);
		}
	}
}
