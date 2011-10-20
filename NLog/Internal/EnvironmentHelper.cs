
namespace NLog.Internal
{
	using System;
	using System.Security;

	/// <summary>
	/// Safe way to get environment variables.
	/// </summary>
	public static class EnvironmentHelper
	{
		public static string NewLine
		{
			get
			{
				string newline = Environment.NewLine;
				return newline;
			}
		}

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
	}
}
