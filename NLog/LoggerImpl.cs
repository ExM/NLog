using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using NLog.Common;
using NLog.Config;
using NLog.Filters;
using NLog.Internal;
using NLog.Targets;

namespace NLog
{
	/// <summary>
	/// Implementation of logging engine.
	/// </summary>
	internal static class LoggerImpl
	{
		private const int StackTraceSkipMethods = 0;
		private static readonly Assembly nlogAssembly = typeof(LoggerImpl).Assembly;
		private static readonly Assembly mscorlibAssembly = typeof(string).Assembly;
		private static readonly Assembly systemAssembly = typeof(Debug).Assembly;

		internal static void Write(Type loggerType, TargetWithFilterChain targets, LogEventInfo logEvent, LogFactory factory)
		{
			if (targets == null)
			{
				return;
			}

			StackTraceUsage stu = targets.GetStackTraceUsage();

			if (stu != StackTraceUsage.None && !logEvent.HasStackTrace)
			{
				StackTrace stackTrace;
				stackTrace = new StackTrace(StackTraceSkipMethods, stu == StackTraceUsage.WithSource);
				int firstUserFrame = FindCallingMethodOnStackTrace(stackTrace, loggerType);
				logEvent.SetStackTrace(stackTrace, firstUserFrame);
			}


			int originalThreadId = Thread.CurrentThread.ManagedThreadId;
			Action<Exception> exceptionHandler = ex =>
				{
					if (ex != null)
					{
						if (factory.ThrowExceptions && Thread.CurrentThread.ManagedThreadId == originalThreadId)
						{
							throw new NLogRuntimeException("Exception occurred in NLog", ex);
						}
					}
				};

			for (var t = targets; t != null; t = t.NextInChain)
			{
				if (!WriteToTargetWithFilterChain(t, logEvent, exceptionHandler))
				{
					break;
				}
			}
		}

		private static int FindCallingMethodOnStackTrace(StackTrace stackTrace, Type loggerType)
		{
			int firstUserFrame = 0;
			for (int i = 0; i < stackTrace.FrameCount; ++i)
			{
				StackFrame frame = stackTrace.GetFrame(i);
				MethodBase mb = frame.GetMethod();
				Assembly methodAssembly = null;

				if (mb.DeclaringType != null)
				{
					methodAssembly = mb.DeclaringType.Assembly;
				}

				if (SkipAssembly(methodAssembly) || mb.DeclaringType == loggerType)
				{
					firstUserFrame = i + 1;
				}
				else
				{
					if (firstUserFrame != 0)
					{
						break;
					}
				}
			}

			return firstUserFrame;
		}

		private static bool SkipAssembly(Assembly assembly)
		{
			if (assembly == nlogAssembly)
			{
				return true;
			}

			if (assembly == mscorlibAssembly)
			{
				return true;
			}

			if (assembly == systemAssembly)
			{
				return true;
			}

			return false;
		}

		private static bool WriteToTargetWithFilterChain(TargetWithFilterChain targetListHead, LogEventInfo logEvent, Action<Exception> onException)
		{
			Target target = targetListHead.Target;
			FilterResult result = GetFilterResult(targetListHead.FilterChain, logEvent);

			if ((result == FilterResult.Ignore) || (result == FilterResult.IgnoreFinal))
			{
				if (InternalLogger.IsDebugEnabled)
				{
					InternalLogger.Debug("{0}.{1} Rejecting message because of a filter.", logEvent.LoggerName, logEvent.Level);
				}

				if (result == FilterResult.IgnoreFinal)
				{
					return false;
				}

				return true;
			}

			target.WriteAsyncLogEvent(logEvent.WithContinuation(onException));
			if (result == FilterResult.LogFinal)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Gets the filter result.
		/// </summary>
		/// <param name="filterChain">The filter chain.</param>
		/// <param name="logEvent">The log event.</param>
		/// <returns>The result of the filter.</returns>
		private static FilterResult GetFilterResult(IEnumerable<Filter> filterChain, LogEventInfo logEvent)
		{
			FilterResult result = FilterResult.Neutral;

			try
			{
				foreach (Filter f in filterChain)
				{
					result = f.GetFilterResult(logEvent);
					if (result != FilterResult.Neutral)
					{
						break;
					}
				}

				return result;
			}
			catch (Exception exception)
			{
				if (exception.MustBeRethrown())
				{
					throw;
				}

				InternalLogger.Warn("Exception during filter evaluation: {0}", exception);
				return FilterResult.Ignore;
			}
		}
	}
}