using System;
using System.ComponentModel;
using NLog.Internal;

namespace NLog
{
	/// <summary>
	/// Provides logging interface and utility functions.
	/// </summary>
	public partial class Logger
	{
		#region Log() overloads
		/// <overloads>
		/// Writes the diagnostic message at the specified level using the specified format provider and format parameters.
		/// </overloads>
		/// <summary>
		/// Writes the diagnostic message at the specified level.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="level">The log level.</param>
		/// <param name="value">The value to be written.</param>
		public void Log<T>(LogLevel level, T value)
		{
			if(IsEnabled(level))
				WriteToTargets(level, null, value);
		}

		/// <summary>
		/// Writes the diagnostic message at the specified level.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="level">The log level.</param>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="value">The value to be written.</param>
		public void Log<T>(LogLevel level, IFormatProvider formatProvider, T value)
		{
			if(IsEnabled(level))
				WriteToTargets(level, formatProvider, value);
		}

		/// <summary>
		/// Writes the diagnostic message at the specified level.
		/// </summary>
		/// <param name="level">The log level.</param>
		/// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
		public void Log(LogLevel level, Func<string> messageFunc)
		{
			if(IsEnabled(level))
			{
				if(messageFunc == null)
					throw new ArgumentNullException("messageFunc");
					
				WriteToTargets(level, null, messageFunc());
			}
		}

		/// <summary>
		/// Writes the diagnostic message and exception at the specified level.
		/// </summary>
		/// <param name="level">The log level.</param>
		/// <param name="message">A <see langword="string" /> to be written.</param>
		/// <param name="exception">An exception to be logged.</param>
		public void LogException(LogLevel level, [Localizable(false)] string message, Exception exception)
		{
			if(IsEnabled(level))
				WriteToTargets(level, message, exception);
		}

		/// <summary>
		/// Writes the diagnostic message at the specified level using the specified parameters and formatting them with the supplied format provider.
		/// </summary>
		/// <param name="level">The log level.</param>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
		public void Log(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
		{
			if(IsEnabled(level))
				WriteToTargets(level, formatProvider, message, args); 
		}

		/// <summary>
		/// Writes the diagnostic message at the specified level.
		/// </summary>
		/// <param name="level">The log level.</param>
		/// <param name="message">Log message.</param>
		public void Log(LogLevel level, [Localizable(false)] string message) 
		{
			if(IsEnabled(level))
				WriteToTargets(level, null, message);
		}

		/// <summary>
		/// Writes the diagnostic message at the specified level using the specified parameters.
		/// </summary>
		/// <param name="level">The log level.</param>
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
		public void Log(LogLevel level, [Localizable(false)] string message, params object[] args) 
		{
			if(IsEnabled(level))
				WriteToTargets(level, null, message, args);
		}

		/// <summary>
		/// Writes the diagnostic message at the specified level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <param name="level">The log level.</param>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		public void Log<TArg1>(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1)
		{
			if(IsEnabled(level))
				WriteToTargets(level, formatProvider, message, new object[] { arg1 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the specified level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <param name="level">The log level.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		public void Log<TArg1>(LogLevel level, [Localizable(false)] string message,
			TArg1 arg1)
		{
			if(IsEnabled(level))
				WriteToTargets(level, null, message, new object[] { arg1 });
		}

		/// <summary>
		/// Writes the diagnostic message at the specified level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <param name="level">The log level.</param>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		public void Log<TArg1, TArg2>(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2)
		{
			if(IsEnabled(level))
				WriteToTargets(level, formatProvider, message, new object[] { arg1, arg2 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the specified level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <param name="level">The log level.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		public void Log<TArg1, TArg2>(LogLevel level, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2)
		{
			if(IsEnabled(level))
				WriteToTargets(level, null, message, new object[] { arg1, arg2 });
		}

		/// <summary>
		/// Writes the diagnostic message at the specified level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <param name="level">The log level.</param>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		public void Log<TArg1, TArg2, TArg3>(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			if(IsEnabled(level))
				WriteToTargets(level, formatProvider, message, new object[] { arg1, arg2, arg3 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the specified level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <param name="level">The log level.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		public void Log<TArg1, TArg2, TArg3>(LogLevel level, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			if(IsEnabled(level))
				WriteToTargets(level, null, message, new object[] { arg1, arg2, arg3 });
		}

		/// <summary>
		/// Writes the diagnostic message at the specified level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <param name="level">The log level.</param>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		public void Log<TArg1, TArg2, TArg3, TArg4>(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			if(IsEnabled(level))
				WriteToTargets(level, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the specified level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <param name="level">The log level.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		public void Log<TArg1, TArg2, TArg3, TArg4>(LogLevel level, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			if(IsEnabled(level))
				WriteToTargets(level, null, message, new object[] { arg1, arg2, arg3, arg4 });
		}

		/// <summary>
		/// Writes the diagnostic message at the specified level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <typeparam name="TArg5">The type of the argument.</typeparam>
		/// <param name="level">The log level.</param>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		/// <param name="arg5">The argument to format.</param>
		public void Log<TArg1, TArg2, TArg3, TArg4, TArg5>(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			if(IsEnabled(level))
				WriteToTargets(level, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the specified level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <typeparam name="TArg5">The type of the argument.</typeparam>
		/// <param name="level">The log level.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		/// <param name="arg5">The argument to format.</param>
		public void Log<TArg1, TArg2, TArg3, TArg4, TArg5>(LogLevel level, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			if(IsEnabled(level))
				WriteToTargets(level, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 });
		}
		#endregion

		#region Trace() overloads
		
		private volatile bool _isTraceEnabled;
		
		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Trace</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Trace</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsTraceEnabled
		{
			get
			{
				return _isTraceEnabled;
			}
		}
		
		/// <overloads>
		/// Writes the diagnostic message at the <c>Trace</c> level using the specified format provider and format parameters.
		/// </overloads>
		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="value">The value to be written.</param>
		public void Trace<T>(T value)
		{
			if(_isTraceEnabled)
				WriteToTargets(LogLevel.Trace, null, value);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="value">The value to be written.</param>
		public void Trace<T>(IFormatProvider formatProvider, T value)
		{
			if(_isTraceEnabled)
				WriteToTargets(LogLevel.Trace, formatProvider, value);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level.
		/// </summary>
		/// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
		public void Trace(Func<string> messageFunc)
		{
			if(_isTraceEnabled)
			{
				if(messageFunc == null)
					throw new ArgumentNullException("messageFunc");

				WriteToTargets(LogLevel.Trace, null, messageFunc());
			}
		}

		/// <summary>
		/// Writes the diagnostic message and exception at the <c>Trace</c> level.
		/// </summary>
		/// <param name="message">A <see langword="string" /> to be written.</param>
		/// <param name="exception">An exception to be logged.</param>
		public void TraceException([Localizable(false)] string message, Exception exception)
		{
			if(_isTraceEnabled)
				WriteToTargets(LogLevel.Trace, message, exception);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters and formatting them with the supplied format provider.
		/// </summary>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
		public void Trace(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
		{
			if(_isTraceEnabled)
				WriteToTargets(LogLevel.Trace, formatProvider, message, args); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level.
		/// </summary>
		/// <param name="message">Log message.</param>
		public void Trace([Localizable(false)] string message) 
		{
			if(_isTraceEnabled)
				WriteToTargets(LogLevel.Trace, null, message);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
		/// </summary>
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
		public void Trace([Localizable(false)] string message, params object[] args) 
		{
			if(_isTraceEnabled)
				WriteToTargets(LogLevel.Trace, null, message, args);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		public void Trace<TArg1>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1)
		{
			if(_isTraceEnabled)
				WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { arg1 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		public void Trace<TArg1>([Localizable(false)] string message,
			TArg1 arg1)
		{
			if(_isTraceEnabled)
				WriteToTargets(LogLevel.Trace, null, message, new object[] { arg1 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		public void Trace<TArg1, TArg2>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2)
		{
			if(_isTraceEnabled)
				WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { arg1, arg2 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		public void Trace<TArg1, TArg2>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2)
		{
			if(_isTraceEnabled)
				WriteToTargets(LogLevel.Trace, null, message, new object[] { arg1, arg2 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		public void Trace<TArg1, TArg2, TArg3>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			if(_isTraceEnabled)
				WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { arg1, arg2, arg3 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		public void Trace<TArg1, TArg2, TArg3>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			if(_isTraceEnabled)
				WriteToTargets(LogLevel.Trace, null, message, new object[] { arg1, arg2, arg3 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		public void Trace<TArg1, TArg2, TArg3, TArg4>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			if(_isTraceEnabled)
				WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		public void Trace<TArg1, TArg2, TArg3, TArg4>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			if(_isTraceEnabled)
				WriteToTargets(LogLevel.Trace, null, message, new object[] { arg1, arg2, arg3, arg4 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <typeparam name="TArg5">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		/// <param name="arg5">The argument to format.</param>
		public void Trace<TArg1, TArg2, TArg3, TArg4, TArg5>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			if(_isTraceEnabled)
				WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <typeparam name="TArg5">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		/// <param name="arg5">The argument to format.</param>
		public void Trace<TArg1, TArg2, TArg3, TArg4, TArg5>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			if(_isTraceEnabled)
				WriteToTargets(LogLevel.Trace, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 });
		}
		#endregion

		#region Debug() overloads
		
		private volatile bool _isDebugEnabled;
		
		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Debug</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Trace</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsDebugEnabled
		{
			get
			{
				return _isDebugEnabled;
			}
		}
		
		/// <overloads>
		/// Writes the diagnostic message at the <c>Debug</c> level using the specified format provider and format parameters.
		/// </overloads>
		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="value">The value to be written.</param>
		public void Debug<T>(T value)
		{
			if(_isDebugEnabled)
				WriteToTargets(LogLevel.Debug, null, value);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="value">The value to be written.</param>
		public void Debug<T>(IFormatProvider formatProvider, T value)
		{
			if(_isDebugEnabled)
				WriteToTargets(LogLevel.Debug, formatProvider, value);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level.
		/// </summary>
		/// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
		public void Debug(Func<string> messageFunc)
		{
			if(_isDebugEnabled)
			{
				if(messageFunc == null)
					throw new ArgumentNullException("messageFunc");

				WriteToTargets(LogLevel.Debug, null, messageFunc());
			}
		}

		/// <summary>
		/// Writes the diagnostic message and exception at the <c>Debug</c> level.
		/// </summary>
		/// <param name="message">A <see langword="string" /> to be written.</param>
		/// <param name="exception">An exception to be logged.</param>
		public void DebugException([Localizable(false)] string message, Exception exception)
		{
			if(_isDebugEnabled)
				WriteToTargets(LogLevel.Debug, message, exception);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters and formatting them with the supplied format provider.
		/// </summary>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
		public void Debug(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
		{
			if(_isDebugEnabled)
				WriteToTargets(LogLevel.Debug, formatProvider, message, args); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level.
		/// </summary>
		/// <param name="message">Log message.</param>
		public void Debug([Localizable(false)] string message) 
		{
			if(_isDebugEnabled)
				WriteToTargets(LogLevel.Debug, null, message);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
		/// </summary>
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
		public void Debug([Localizable(false)] string message, params object[] args) 
		{
			if(_isDebugEnabled)
				WriteToTargets(LogLevel.Debug, null, message, args);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		public void Debug<TArg1>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1)
		{
			if(_isDebugEnabled)
				WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { arg1 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		public void Debug<TArg1>([Localizable(false)] string message,
			TArg1 arg1)
		{
			if(_isDebugEnabled)
				WriteToTargets(LogLevel.Debug, null, message, new object[] { arg1 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		public void Debug<TArg1, TArg2>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2)
		{
			if(_isDebugEnabled)
				WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { arg1, arg2 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		public void Debug<TArg1, TArg2>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2)
		{
			if(_isDebugEnabled)
				WriteToTargets(LogLevel.Debug, null, message, new object[] { arg1, arg2 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		public void Debug<TArg1, TArg2, TArg3>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			if(_isDebugEnabled)
				WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { arg1, arg2, arg3 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		public void Debug<TArg1, TArg2, TArg3>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			if(_isDebugEnabled)
				WriteToTargets(LogLevel.Debug, null, message, new object[] { arg1, arg2, arg3 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		public void Debug<TArg1, TArg2, TArg3, TArg4>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			if(_isDebugEnabled)
				WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		public void Debug<TArg1, TArg2, TArg3, TArg4>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			if(_isDebugEnabled)
				WriteToTargets(LogLevel.Debug, null, message, new object[] { arg1, arg2, arg3, arg4 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <typeparam name="TArg5">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		/// <param name="arg5">The argument to format.</param>
		public void Debug<TArg1, TArg2, TArg3, TArg4, TArg5>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			if(_isDebugEnabled)
				WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <typeparam name="TArg5">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		/// <param name="arg5">The argument to format.</param>
		public void Debug<TArg1, TArg2, TArg3, TArg4, TArg5>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			if(_isDebugEnabled)
				WriteToTargets(LogLevel.Debug, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 });
		}
		#endregion

		#region Info() overloads
		
		private volatile bool _isInfoEnabled;
		
		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Info</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Trace</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsInfoEnabled
		{
			get
			{
				return _isInfoEnabled;
			}
		}
		
		/// <overloads>
		/// Writes the diagnostic message at the <c>Info</c> level using the specified format provider and format parameters.
		/// </overloads>
		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="value">The value to be written.</param>
		public void Info<T>(T value)
		{
			if(_isInfoEnabled)
				WriteToTargets(LogLevel.Info, null, value);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="value">The value to be written.</param>
		public void Info<T>(IFormatProvider formatProvider, T value)
		{
			if(_isInfoEnabled)
				WriteToTargets(LogLevel.Info, formatProvider, value);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level.
		/// </summary>
		/// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
		public void Info(Func<string> messageFunc)
		{
			if(_isInfoEnabled)
			{
				if(messageFunc == null)
					throw new ArgumentNullException("messageFunc");

				WriteToTargets(LogLevel.Info, null, messageFunc());
			}
		}

		/// <summary>
		/// Writes the diagnostic message and exception at the <c>Info</c> level.
		/// </summary>
		/// <param name="message">A <see langword="string" /> to be written.</param>
		/// <param name="exception">An exception to be logged.</param>
		public void InfoException([Localizable(false)] string message, Exception exception)
		{
			if(_isInfoEnabled)
				WriteToTargets(LogLevel.Info, message, exception);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level using the specified parameters and formatting them with the supplied format provider.
		/// </summary>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
		public void Info(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
		{
			if(_isInfoEnabled)
				WriteToTargets(LogLevel.Info, formatProvider, message, args); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level.
		/// </summary>
		/// <param name="message">Log message.</param>
		public void Info([Localizable(false)] string message) 
		{
			if(_isInfoEnabled)
				WriteToTargets(LogLevel.Info, null, message);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level using the specified parameters.
		/// </summary>
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
		public void Info([Localizable(false)] string message, params object[] args) 
		{
			if(_isInfoEnabled)
				WriteToTargets(LogLevel.Info, null, message, args);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		public void Info<TArg1>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1)
		{
			if(_isInfoEnabled)
				WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { arg1 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		public void Info<TArg1>([Localizable(false)] string message,
			TArg1 arg1)
		{
			if(_isInfoEnabled)
				WriteToTargets(LogLevel.Info, null, message, new object[] { arg1 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		public void Info<TArg1, TArg2>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2)
		{
			if(_isInfoEnabled)
				WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { arg1, arg2 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		public void Info<TArg1, TArg2>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2)
		{
			if(_isInfoEnabled)
				WriteToTargets(LogLevel.Info, null, message, new object[] { arg1, arg2 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		public void Info<TArg1, TArg2, TArg3>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			if(_isInfoEnabled)
				WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { arg1, arg2, arg3 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		public void Info<TArg1, TArg2, TArg3>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			if(_isInfoEnabled)
				WriteToTargets(LogLevel.Info, null, message, new object[] { arg1, arg2, arg3 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		public void Info<TArg1, TArg2, TArg3, TArg4>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			if(_isInfoEnabled)
				WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		public void Info<TArg1, TArg2, TArg3, TArg4>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			if(_isInfoEnabled)
				WriteToTargets(LogLevel.Info, null, message, new object[] { arg1, arg2, arg3, arg4 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <typeparam name="TArg5">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		/// <param name="arg5">The argument to format.</param>
		public void Info<TArg1, TArg2, TArg3, TArg4, TArg5>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			if(_isInfoEnabled)
				WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <typeparam name="TArg5">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		/// <param name="arg5">The argument to format.</param>
		public void Info<TArg1, TArg2, TArg3, TArg4, TArg5>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			if(_isInfoEnabled)
				WriteToTargets(LogLevel.Info, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 });
		}
		#endregion

		#region Warn() overloads
		
		private volatile bool _isWarnEnabled;
		
		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Warn</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Trace</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsWarnEnabled
		{
			get
			{
				return _isWarnEnabled;
			}
		}
		
		/// <overloads>
		/// Writes the diagnostic message at the <c>Warn</c> level using the specified format provider and format parameters.
		/// </overloads>
		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="value">The value to be written.</param>
		public void Warn<T>(T value)
		{
			if(_isWarnEnabled)
				WriteToTargets(LogLevel.Warn, null, value);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="value">The value to be written.</param>
		public void Warn<T>(IFormatProvider formatProvider, T value)
		{
			if(_isWarnEnabled)
				WriteToTargets(LogLevel.Warn, formatProvider, value);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level.
		/// </summary>
		/// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
		public void Warn(Func<string> messageFunc)
		{
			if(_isWarnEnabled)
			{
				if(messageFunc == null)
					throw new ArgumentNullException("messageFunc");

				WriteToTargets(LogLevel.Warn, null, messageFunc());
			}
		}

		/// <summary>
		/// Writes the diagnostic message and exception at the <c>Warn</c> level.
		/// </summary>
		/// <param name="message">A <see langword="string" /> to be written.</param>
		/// <param name="exception">An exception to be logged.</param>
		public void WarnException([Localizable(false)] string message, Exception exception)
		{
			if(_isWarnEnabled)
				WriteToTargets(LogLevel.Warn, message, exception);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level using the specified parameters and formatting them with the supplied format provider.
		/// </summary>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
		public void Warn(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
		{
			if(_isWarnEnabled)
				WriteToTargets(LogLevel.Warn, formatProvider, message, args); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level.
		/// </summary>
		/// <param name="message">Log message.</param>
		public void Warn([Localizable(false)] string message) 
		{
			if(_isWarnEnabled)
				WriteToTargets(LogLevel.Warn, null, message);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level using the specified parameters.
		/// </summary>
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
		public void Warn([Localizable(false)] string message, params object[] args) 
		{
			if(_isWarnEnabled)
				WriteToTargets(LogLevel.Warn, null, message, args);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		public void Warn<TArg1>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1)
		{
			if(_isWarnEnabled)
				WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { arg1 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		public void Warn<TArg1>([Localizable(false)] string message,
			TArg1 arg1)
		{
			if(_isWarnEnabled)
				WriteToTargets(LogLevel.Warn, null, message, new object[] { arg1 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		public void Warn<TArg1, TArg2>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2)
		{
			if(_isWarnEnabled)
				WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { arg1, arg2 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		public void Warn<TArg1, TArg2>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2)
		{
			if(_isWarnEnabled)
				WriteToTargets(LogLevel.Warn, null, message, new object[] { arg1, arg2 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		public void Warn<TArg1, TArg2, TArg3>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			if(_isWarnEnabled)
				WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { arg1, arg2, arg3 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		public void Warn<TArg1, TArg2, TArg3>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			if(_isWarnEnabled)
				WriteToTargets(LogLevel.Warn, null, message, new object[] { arg1, arg2, arg3 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		public void Warn<TArg1, TArg2, TArg3, TArg4>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			if(_isWarnEnabled)
				WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		public void Warn<TArg1, TArg2, TArg3, TArg4>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			if(_isWarnEnabled)
				WriteToTargets(LogLevel.Warn, null, message, new object[] { arg1, arg2, arg3, arg4 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <typeparam name="TArg5">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		/// <param name="arg5">The argument to format.</param>
		public void Warn<TArg1, TArg2, TArg3, TArg4, TArg5>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			if(_isWarnEnabled)
				WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <typeparam name="TArg5">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		/// <param name="arg5">The argument to format.</param>
		public void Warn<TArg1, TArg2, TArg3, TArg4, TArg5>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			if(_isWarnEnabled)
				WriteToTargets(LogLevel.Warn, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 });
		}
		#endregion

		#region Error() overloads
		
		private volatile bool _isErrorEnabled;
		
		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Error</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Trace</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsErrorEnabled
		{
			get
			{
				return _isErrorEnabled;
			}
		}
		
		/// <overloads>
		/// Writes the diagnostic message at the <c>Error</c> level using the specified format provider and format parameters.
		/// </overloads>
		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="value">The value to be written.</param>
		public void Error<T>(T value)
		{
			if(_isErrorEnabled)
				WriteToTargets(LogLevel.Error, null, value);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="value">The value to be written.</param>
		public void Error<T>(IFormatProvider formatProvider, T value)
		{
			if(_isErrorEnabled)
				WriteToTargets(LogLevel.Error, formatProvider, value);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level.
		/// </summary>
		/// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
		public void Error(Func<string> messageFunc)
		{
			if(_isErrorEnabled)
			{
				if(messageFunc == null)
					throw new ArgumentNullException("messageFunc");

				WriteToTargets(LogLevel.Error, null, messageFunc());
			}
		}

		/// <summary>
		/// Writes the diagnostic message and exception at the <c>Error</c> level.
		/// </summary>
		/// <param name="message">A <see langword="string" /> to be written.</param>
		/// <param name="exception">An exception to be logged.</param>
		public void ErrorException([Localizable(false)] string message, Exception exception)
		{
			if(_isErrorEnabled)
				WriteToTargets(LogLevel.Error, message, exception);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level using the specified parameters and formatting them with the supplied format provider.
		/// </summary>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
		public void Error(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
		{
			if(_isErrorEnabled)
				WriteToTargets(LogLevel.Error, formatProvider, message, args); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level.
		/// </summary>
		/// <param name="message">Log message.</param>
		public void Error([Localizable(false)] string message) 
		{
			if(_isErrorEnabled)
				WriteToTargets(LogLevel.Error, null, message);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level using the specified parameters.
		/// </summary>
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
		public void Error([Localizable(false)] string message, params object[] args) 
		{
			if(_isErrorEnabled)
				WriteToTargets(LogLevel.Error, null, message, args);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		public void Error<TArg1>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1)
		{
			if(_isErrorEnabled)
				WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { arg1 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		public void Error<TArg1>([Localizable(false)] string message,
			TArg1 arg1)
		{
			if(_isErrorEnabled)
				WriteToTargets(LogLevel.Error, null, message, new object[] { arg1 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		public void Error<TArg1, TArg2>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2)
		{
			if(_isErrorEnabled)
				WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { arg1, arg2 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		public void Error<TArg1, TArg2>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2)
		{
			if(_isErrorEnabled)
				WriteToTargets(LogLevel.Error, null, message, new object[] { arg1, arg2 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		public void Error<TArg1, TArg2, TArg3>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			if(_isErrorEnabled)
				WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { arg1, arg2, arg3 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		public void Error<TArg1, TArg2, TArg3>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			if(_isErrorEnabled)
				WriteToTargets(LogLevel.Error, null, message, new object[] { arg1, arg2, arg3 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		public void Error<TArg1, TArg2, TArg3, TArg4>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			if(_isErrorEnabled)
				WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		public void Error<TArg1, TArg2, TArg3, TArg4>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			if(_isErrorEnabled)
				WriteToTargets(LogLevel.Error, null, message, new object[] { arg1, arg2, arg3, arg4 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <typeparam name="TArg5">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		/// <param name="arg5">The argument to format.</param>
		public void Error<TArg1, TArg2, TArg3, TArg4, TArg5>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			if(_isErrorEnabled)
				WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <typeparam name="TArg5">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		/// <param name="arg5">The argument to format.</param>
		public void Error<TArg1, TArg2, TArg3, TArg4, TArg5>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			if(_isErrorEnabled)
				WriteToTargets(LogLevel.Error, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 });
		}
		#endregion

		#region Fatal() overloads
		
		private volatile bool _isFatalEnabled;
		
		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Fatal</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Trace</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsFatalEnabled
		{
			get
			{
				return _isFatalEnabled;
			}
		}
		
		/// <overloads>
		/// Writes the diagnostic message at the <c>Fatal</c> level using the specified format provider and format parameters.
		/// </overloads>
		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="value">The value to be written.</param>
		public void Fatal<T>(T value)
		{
			if(_isFatalEnabled)
				WriteToTargets(LogLevel.Fatal, null, value);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="value">The value to be written.</param>
		public void Fatal<T>(IFormatProvider formatProvider, T value)
		{
			if(_isFatalEnabled)
				WriteToTargets(LogLevel.Fatal, formatProvider, value);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level.
		/// </summary>
		/// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
		public void Fatal(Func<string> messageFunc)
		{
			if(_isFatalEnabled)
			{
				if(messageFunc == null)
					throw new ArgumentNullException("messageFunc");

				WriteToTargets(LogLevel.Fatal, null, messageFunc());
			}
		}

		/// <summary>
		/// Writes the diagnostic message and exception at the <c>Fatal</c> level.
		/// </summary>
		/// <param name="message">A <see langword="string" /> to be written.</param>
		/// <param name="exception">An exception to be logged.</param>
		public void FatalException([Localizable(false)] string message, Exception exception)
		{
			if(_isFatalEnabled)
				WriteToTargets(LogLevel.Fatal, message, exception);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameters and formatting them with the supplied format provider.
		/// </summary>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
		public void Fatal(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
		{
			if(_isFatalEnabled)
				WriteToTargets(LogLevel.Fatal, formatProvider, message, args); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level.
		/// </summary>
		/// <param name="message">Log message.</param>
		public void Fatal([Localizable(false)] string message) 
		{
			if(_isFatalEnabled)
				WriteToTargets(LogLevel.Fatal, null, message);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameters.
		/// </summary>
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
		public void Fatal([Localizable(false)] string message, params object[] args) 
		{
			if(_isFatalEnabled)
				WriteToTargets(LogLevel.Fatal, null, message, args);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		public void Fatal<TArg1>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1)
		{
			if(_isFatalEnabled)
				WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { arg1 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		public void Fatal<TArg1>([Localizable(false)] string message,
			TArg1 arg1)
		{
			if(_isFatalEnabled)
				WriteToTargets(LogLevel.Fatal, null, message, new object[] { arg1 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		public void Fatal<TArg1, TArg2>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2)
		{
			if(_isFatalEnabled)
				WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { arg1, arg2 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		public void Fatal<TArg1, TArg2>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2)
		{
			if(_isFatalEnabled)
				WriteToTargets(LogLevel.Fatal, null, message, new object[] { arg1, arg2 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		public void Fatal<TArg1, TArg2, TArg3>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			if(_isFatalEnabled)
				WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { arg1, arg2, arg3 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		public void Fatal<TArg1, TArg2, TArg3>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			if(_isFatalEnabled)
				WriteToTargets(LogLevel.Fatal, null, message, new object[] { arg1, arg2, arg3 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		public void Fatal<TArg1, TArg2, TArg3, TArg4>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			if(_isFatalEnabled)
				WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		public void Fatal<TArg1, TArg2, TArg3, TArg4>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			if(_isFatalEnabled)
				WriteToTargets(LogLevel.Fatal, null, message, new object[] { arg1, arg2, arg3, arg4 });
		}
		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameter and formatting it with the supplied format provider.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <typeparam name="TArg5">The type of the argument.</typeparam>
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		/// <param name="arg5">The argument to format.</param>
		public void Fatal<TArg1, TArg2, TArg3, TArg4, TArg5>(IFormatProvider formatProvider, [Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			if(_isFatalEnabled)
				WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }); 
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameter.
		/// </summary>
		/// <typeparam name="TArg1">The type of the argument.</typeparam>
		/// <typeparam name="TArg2">The type of the argument.</typeparam>
		/// <typeparam name="TArg3">The type of the argument.</typeparam>
		/// <typeparam name="TArg4">The type of the argument.</typeparam>
		/// <typeparam name="TArg5">The type of the argument.</typeparam>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="arg1">The argument to format.</param>
		/// <param name="arg2">The argument to format.</param>
		/// <param name="arg3">The argument to format.</param>
		/// <param name="arg4">The argument to format.</param>
		/// <param name="arg5">The argument to format.</param>
		public void Fatal<TArg1, TArg2, TArg3, TArg4, TArg5>([Localizable(false)] string message,
			TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			if(_isFatalEnabled)
				WriteToTargets(LogLevel.Fatal, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 });
		}
		#endregion
	}
}
