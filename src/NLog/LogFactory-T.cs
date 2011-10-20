
namespace NLog
{
	using System.Diagnostics;
	using System.Runtime.CompilerServices;

	/// <summary>
	/// Specialized LogFactory that can return instances of custom logger types.
	/// </summary>
	/// <typeparam name="T">The type of the logger to be returned. Must inherit from <see cref="Logger"/>.</typeparam>
	public class LogFactory<T> : LogFactory 
		where T : Logger
	{
		/// <summary>
		/// Gets the logger.
		/// </summary>
		/// <param name="name">The logger name.</param>
		/// <returns>An instance of <typeparamref name="T"/>.</returns>
		public new T GetLogger(string name)
		{
			return (T)this.GetLogger(name, typeof(T));
		}

#if !NET_CF
		/// <summary>
		/// Gets the logger named after the currently-being-initialized class.
		/// </summary>
		/// <returns>The logger.</returns>
		/// <remarks>This is a slow-running method. 
		/// Make sure you're not doing this in a loop.</remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Backwards compatibility")]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public new T GetCurrentClassLogger()
		{
#if SILVERLIGHT
			StackFrame frame = new StackFrame(1);
#else
			StackFrame frame = new StackFrame(1, false);
#endif

			return this.GetLogger(frame.GetMethod().DeclaringType.FullName);
		}
#endif
	}
}
