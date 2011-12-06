using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NLog.Internal;

namespace NLog.Common
{
	/// <summary>
	/// Helpers for asynchronous operations.
	/// </summary>
	public static class AsyncHelpers
	{
		/// <summary>
		/// Iterates over all items in the given collection and runs the specified action
		/// in sequence (each action executes only after the preceding one has completed without an error).
		/// </summary>
		/// <typeparam name="T">Type of each item.</typeparam>
		/// <param name="items">The items to iterate.</param>
		/// <param name="asyncContinuation">The asynchronous continuation to invoke once all items
		/// have been iterated.</param>
		/// <param name="action">The action to invoke for each item.</param>
		public static void ForEachItemSequentially<T>(IEnumerable<T> items, Action<Exception> asyncContinuation, Action<T, Action<Exception>> action)
		{
			action = ExceptionGuard(action);
			Action<Exception> invokeNext = null;
			IEnumerator<T> enumerator = items.GetEnumerator();

			invokeNext = ex =>
			{
				if (ex != null)
				{
					asyncContinuation(ex);
					return;
				}

				if (!enumerator.MoveNext())
				{
					asyncContinuation(null);
					return;
				}

				action(enumerator.Current, PreventMultipleCalls(invokeNext));
			};

			invokeNext(null);
		}

		/// <summary>
		/// Repeats the specified asynchronous action multiple times and invokes asynchronous continuation at the end.
		/// </summary>
		/// <param name="repeatCount">The repeat count.</param>
		/// <param name="asyncContinuation">The asynchronous continuation to invoke at the end.</param>
		/// <param name="action">The action to invoke.</param>
		public static void Repeat(int repeatCount, Action<Exception> asyncContinuation, Action<Action<Exception>> action)
		{
			action = ExceptionGuard(action);
			Action<Exception> invokeNext = null;
			int remaining = repeatCount;

			invokeNext = ex =>
				{
					if (ex != null)
					{
						asyncContinuation(ex);
						return;
					}

					if (remaining-- <= 0)
					{
						asyncContinuation(null);
						return;
					}

					action(PreventMultipleCalls(invokeNext));
				};

			invokeNext(null);
		}

		/// <summary>
		/// Modifies the continuation by pre-pending given action to execute just before it.
		/// </summary>
		/// <param name="asyncContinuation">The async continuation.</param>
		/// <param name="action">The action to pre-pend.</param>
		/// <returns>Continuation which will execute the given action before forwarding to the actual continuation.</returns>
		public static Action<Exception> PrecededBy(Action<Exception> asyncContinuation, Action<Action<Exception>> action)
		{
			action = ExceptionGuard(action);

			Action<Exception> continuation =
				ex =>
				{
					if (ex != null)
					{
						// if got exception from from original invocation, don't execute action
						asyncContinuation(ex);
						return;
					}

					// call the action and continue
					action(PreventMultipleCalls(asyncContinuation));
				};

			return continuation;
		}

		/// <summary>
		/// Attaches a timeout to a continuation which will invoke the continuation when the specified
		/// timeout has elapsed.
		/// </summary>
		/// <param name="asyncContinuation">The asynchronous continuation.</param>
		/// <param name="timeout">The timeout.</param>
		/// <returns>Wrapped continuation.</returns>
		public static Action<Exception> WithTimeout(Action<Exception> asyncContinuation, TimeSpan timeout)
		{
			return new TimeoutContinuation(asyncContinuation, timeout).Function;
		}

		/// <summary>
		/// Iterates over all items in the given collection and runs the specified action
		/// in parallel (each action executes on a thread from thread pool).
		/// </summary>
		/// <typeparam name="T">Type of each item.</typeparam>
		/// <param name="values">The items to iterate.</param>
		/// <param name="asyncContinuation">The asynchronous continuation to invoke once all items
		/// have been iterated.</param>
		/// <param name="action">The action to invoke for each item.</param>
		public static void ForEachItemInParallel<T>(IEnumerable<T> values, Action<Exception> asyncContinuation, Action<T, Action<Exception>> action)
		{
			action = ExceptionGuard(action);

			var items = new List<T>(values);
			int remaining = items.Count;
			var exceptions = new List<Exception>();

			InternalLogger.Trace("ForEachItemInParallel() {0} items", items.Count);

			if (remaining == 0)
			{
				asyncContinuation(null);
				return;
			}

			Action<Exception> continuation =
				ex =>
					{
						InternalLogger.Trace("Continuation invoked: {0}", ex);
						int r;

						if (ex != null)
						{
							lock (exceptions)
							{
								exceptions.Add(ex);
							}
						}

						r = Interlocked.Decrement(ref remaining);
						InternalLogger.Trace("Parallel task completed. {0} items remaining", r);
						if (r == 0)
						{
							asyncContinuation(GetCombinedException(exceptions));
						}
					};

			foreach (T item in items)
			{
				T itemCopy = item;

				ThreadPool.QueueUserWorkItem(s => action(itemCopy, PreventMultipleCalls(continuation)));
			}
		}

		/// <summary>
		/// Wraps the continuation with a guard which will only make sure that the continuation function
		/// is invoked only once.
		/// </summary>
		/// <param name="asyncContinuation">The asynchronous continuation.</param>
		/// <returns>Wrapped asynchronous continuation.</returns>
		public static Action<Exception> PreventMultipleCalls(Action<Exception> asyncContinuation)
		{
			if (asyncContinuation.Target is SingleCallContinuation)
			{
				return asyncContinuation;
			}
			return new SingleCallContinuation(asyncContinuation).Function;
		}

		/// <summary>
		/// Gets the combined exception from all exceptions in the list.
		/// </summary>
		/// <param name="exceptions">The exceptions.</param>
		/// <returns>Combined exception or null if no exception was thrown.</returns>
		public static Exception GetCombinedException(IList<Exception> exceptions)
		{
			if (exceptions.Count == 0)
			{
				return null;
			}

			if (exceptions.Count == 1)
			{
				return exceptions[0];
			}

			var sb = new StringBuilder();
			string separator = string.Empty;
			string newline = Environment.NewLine;
			foreach (var ex in exceptions)
			{
				sb.Append(separator);
				sb.Append(ex.ToString());
				sb.Append(newline);
				separator = newline;
			}

			return new NLogRuntimeException("Got multiple exceptions:\r\n" + sb);
		}

		private static Action<Action<Exception>> ExceptionGuard(Action<Action<Exception>> action)
		{
			return cont =>
			{
				try
				{
					action(cont);
				}
				catch (Exception exception)
				{
					if (exception.MustBeRethrown())
					{
						throw;
					}

					cont(exception);
				}
			};
		}

		internal static Action<T, Action<Exception>> ExceptionGuard<T>(Action<T, Action<Exception>> action)
		{
			return (T argument, Action<Exception> cont) =>
			{
				try
				{
					action(argument, cont);
				}
				catch (Exception exception)
				{
					if (exception.MustBeRethrown())
					{
						throw;
					}

					cont(exception);
				}
			};
		}
	}
}
