
namespace NLog.Common
{
	using System;

	/// <summary>
	/// Asynchronous continuation delegate - function invoked at the end of asynchronous
	/// processing.
	/// </summary>
	/// <param name="exception">Exception during asynchronous processing or null if no exception
	/// was thrown.</param>
	public delegate void AsyncContinuation(Exception exception);
}