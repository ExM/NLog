
namespace NLog.Internal
{
	using System;
	using System.Security;

	/// <summary>
	/// Safe way to get environment variables.
	/// </summary>
	internal static class EnvironmentHelper
	{
		internal static string NewLine
		{
			get
			{
#if !SILVERLIGHT && !NET_CF
				string newline = Environment.NewLine;
#else
				string newline = "\r\n";
#endif

				return newline;
			}
		}

#if !NET_CF && !SILVERLIGHT
		internal static string GetSafeEnvironmentVariable(string name)
		{
			try
			{
				string s = Environment.GetEnvironmentVariable(name);

				if (s == null || s.Length == 0)
				{
					return null;
				}

				return s;
			}
			catch (SecurityException)
			{
				return string.Empty;
			}
		}
#endif
	}
}
