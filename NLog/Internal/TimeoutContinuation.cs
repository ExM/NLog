using System;
using System.Threading;
using NLog.Common;

namespace NLog.Internal
{
	/// <summary>
	/// Wraps Action[Exception] with a timeout.
	/// </summary>
	internal class TimeoutContinuation : IDisposable
	{
		private Action<Exception> asyncContinuation;
		private Timer timeoutTimer;

		/// <summary>
		/// Initializes a new instance of the <see cref="TimeoutContinuation"/> class.
		/// </summary>
		/// <param name="asyncContinuation">The asynchronous continuation.</param>
		/// <param name="timeout">The timeout.</param>
		public TimeoutContinuation(Action<Exception> asyncContinuation, TimeSpan timeout)
		{
			this.asyncContinuation = asyncContinuation;
			this.timeoutTimer = new Timer(this.TimerElapsed, null, timeout, TimeSpan.FromMilliseconds(-1));
		}

		/// <summary>
		/// Continuation function which implements the timeout logic.
		/// </summary>
		/// <param name="exception">The exception.</param>
		public void Function(Exception exception)
		{
			try
			{
				this.StopTimer();

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

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.StopTimer();
			GC.SuppressFinalize(this);
		}

		private static void ReportExceptionInHandler(Exception exception)
		{
			InternalLogger.Error("Exception in asynchronous handler {0}", exception);
		}

		private void StopTimer()
		{
			lock (this)
			{
				if (this.timeoutTimer != null)
				{
					this.timeoutTimer.Dispose();
					this.timeoutTimer = null;
				}
			}
		}

		private void TimerElapsed(object state)
		{
			this.Function(new TimeoutException("Timeout."));
		}
	}
}