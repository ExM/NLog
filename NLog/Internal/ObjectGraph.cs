using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NLog.Config;
using NLog.Common;

namespace NLog.Internal
{
	internal static class ObjectGraph
	{
		public static ISupportsInitialize[] DeepInitialize(object root, LoggingConfiguration cfg, bool throwExceptions)
		{
			InitializeContext context = new InitializeContext(cfg, throwExceptions);
			context.DeepInitialize(root);
			return context.Initialized;
		}

		public static ISupportsInitialize[] DeepInitialize(this IEnumerable<object> roots, LoggingConfiguration cfg, bool throwExceptions)
		{
			InitializeContext context = new InitializeContext(cfg, throwExceptions);
			foreach (object root in roots)
				context.DeepInitialize(root);
			return context.Initialized;
		}

		/// <summary>
		/// find all objects which may need stack trace
		/// and determine maximum
		/// </summary>
		/// <param name="root"></param>
		/// <returns></returns>
		public static StackTraceUsage PrecalculateStackTraceUsage(object root)
		{
			StackTraceUsage result = StackTraceUsage.None;

			foreach (var item in AllChilds<IUsesStackTrace>(root))
			{
				var stu = item.StackTraceUsage;

				if (stu <= result)
					continue;

				result = stu;
				if (result >= StackTraceUsage.Max)
					break;
			}

			return result;
		}

		/// <summary>
		/// determine whether the layout is thread-agnostic
		/// layout is thread agnostic if it is thread-agnostic and 
		/// all its nested objects are thread-agnostic.
		/// </summary>
		/// <param name="root"></param>
		/// <returns></returns>
		public static bool ResolveThreadAgnostic(object root)
		{
			foreach (object item in AllChilds<object>(root))
				if (!item.GetType().IsDefined(typeof(ThreadAgnosticAttribute), true))
					return false;

			return true;
		}

		public static IEnumerable<T> AllChilds<T>(object arg)
			where T: class
		{
			IEnumerable roots = arg as IEnumerable;
			if(roots == null)
			{
				foreach(T item in AllChilds<T>(arg, new HashSet<object>()))
					yield return item;
			}
			else
			{
				HashSet<object> visited = new HashSet<object>();

				foreach(object root in roots)
					foreach(T item in AllChilds<T>(root, visited))
						yield return item;
			}
		}

		private static IEnumerable<T> AllChilds<T>(object o, HashSet<object> visited)
			where T: class
		{
			if (o == null)
				yield break;

			if (!o.GetType().IsDefined(typeof(NLogConfigurationItemAttribute), true))
				yield break;

			if(!visited.Add(o))
				yield break;

			T t = o as T;
			if(t != null)
				yield return t;

			foreach (object ch in OneLevelChilds(o))
				foreach (T ch2 in AllChilds<T>(ch, visited))
					yield return ch2;
		}

		private static object _sync = new object();
		private static Dictionary<Type, PropertyCache> _cacheMap = new Dictionary<Type, PropertyCache>();

		public static IEnumerable<T> OneLevelChilds<T>(object o)
			where T : class
		{
			return GetProperty(o.GetType())
				.EnumChilds<T>(o);
		}

		public static IEnumerable OneLevelChilds(object o)
		{
			return GetProperty(o.GetType())
				.EnumChilds(o);
		}

		public static void CheckRequiredParameters(object o)
		{
			GetProperty(o.GetType())
				.CheckRequiredParameters(o);
		}

		private static PropertyCache GetProperty(Type curT)
		{
			PropertyCache cache;
			lock (_sync)
			{
				if (!_cacheMap.TryGetValue(curT, out cache))
				{
					cache = new PropertyCache(curT);
					_cacheMap.Add(curT, cache);
				}
			}
			return cache;
		}

		private class PropertyCache
		{
			PropertyInfo[] piList;

			public PropertyCache(Type t)
			{
				List<PropertyInfo> list = new List<PropertyInfo>();

				foreach (PropertyInfo prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					if (prop.PropertyType.IsPrimitive || prop.PropertyType.IsEnum || prop.PropertyType == typeof(string))
						continue;

					list.Add(prop);
				}

				piList = list.ToArray();
			}

			public void CheckRequiredParameters(object o)
			{
				for(int i = 0; i < piList.Length; i++)
				{
					var pi = piList[i];
					if (!pi.IsDefined(typeof(RequiredParameterAttribute), false))
						continue;

					object value = pi.GetValue(o, null);
					if (value == null)
						throw new NLogConfigurationException(
							"Required parameter '" + pi.Name + "' on '" + o + "' was not specified.");
				}
			}

			public IEnumerable EnumChilds(object o)
			{
				foreach (var p in piList)
				{
					object val = p.GetValue(o, null);
					if (val == null)
						continue;

					var enumerable = val as IEnumerable;
					if (enumerable == null)
						yield return val;
					else
					{
						foreach (object el in enumerable)
							yield return el;
					}
				}
			}

			public IEnumerable<T> EnumChilds<T>(object o)
				where T : class
			{
				for(int i = 0; i < piList.Length; i++)
				{
					object val = piList[i].GetValue(o, null);
					if (val == null)
						continue;

					var enumerable = val as IEnumerable;
					if (enumerable == null)
					{
						T t = val as T;
						if(t != null)
							yield return t;
					}
					else
					{
						foreach (object el in enumerable)
						{
							T t = el as T;
							if (t != null)
								yield return t;
						}
					}
				}

			}
		}
	}
}
