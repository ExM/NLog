using System;
using System.Threading;
using NLog.Common;

namespace NLog.Internal
{
	/// <summary>
	/// Implements a single-call guard around given continuation function.
	/// </summary>
	internal class SingleCallContinuation
	{
		private Action<Exception> asyncContinuation;

		/// <summary>
		/// Initializes a new instance of the <see cref="SingleCallContinuation"/> class.
		/// </summary>
		/// <param name="asyncContinuation">The asynchronous continuation.</param>
		public SingleCallContinuation(Action<Exception> asyncContinuation)
		{
			this.asyncContinuation = asyncContinuation;
		}

		/// <summary>
		/// Continuation function which implements the single-call guard.
		/// </summary>
		/// <param name="exception">The exception.</param>
		public void Function(Exception exception)
		{
			try
			{
				var cont = Interlocked.Exchange(ref this.asyncContinuation, null);
				if (cont != null)
				{
					cont(exception);
				}
			}
			catch (Exception ex)
			{
				if (ex.MustBeRethrown())
				{
					throw;
				}

				ReportExceptionInHandler(ex);
			}
		}

		private static void ReportExceptionInHandler(Exception exception)
		{
			InternalLogger.Error("Exception in asynchronous handler {0}", exception);
		}
	}
}