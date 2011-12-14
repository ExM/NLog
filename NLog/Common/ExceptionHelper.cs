using System;
using System.Threading;

namespace NLog.Common
{
	/// <summary>
	/// Helper class for dealing with exceptions.
	/// </summary>
	public static class ExceptionHelper
	{
		/// <summary>
		/// Determines whether the exception must be rethrown.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <returns>True if the exception must be rethrown, false otherwise.</returns>
		public static bool MustBeRethrown(this Exception exception)
		{
			return exception is StackOverflowException ||
				exception is ThreadAbortException ||
				exception is OutOfMemoryException;
		}
	}
}
