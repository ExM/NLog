
namespace NLog.Internal
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text;
	using NLog.Common;

	/// <summary>
	/// Reflection helpers.
	/// </summary>
	public static class ReflectionHelpers
	{
		/// <summary>
		/// Gets all usable exported types from the given assembly.
		/// </summary>
		/// <param name="assembly">Assembly to scan.</param>
		/// <returns>Usable types from the given assembly.</returns>
		/// <remarks>Types which cannot be loaded are skipped.</remarks>
		public static Type[] SafeGetTypes(this Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException typeLoadException)
			{
				foreach (var ex in typeLoadException.LoaderExceptions)
				{
					InternalLogger.Warn("Type load exception: {0}", ex);
				}

				var loadedTypes = new List<Type>();
				foreach (var t in typeLoadException.Types)
				{
					if (t != null)
					{
						loadedTypes.Add(t);
					}
				}

				return loadedTypes.ToArray();
			}
		}
	}
}
