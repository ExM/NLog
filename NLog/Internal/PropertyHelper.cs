using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using NLog.Common;
using NLog.Conditions;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.Internal
{
	/// <summary>
	/// Reflection helpers for accessing properties.
	/// </summary>
	internal static class PropertyHelper
	{
		private static Dictionary<Type, Dictionary<string, PropertyInfo>> parameterInfoCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

		internal static void SetPropertyFromString(object o, string name, string value, LoggingConfiguration cfg)
		{
			InternalLogger.Debug("Setting '{0}.{1}' to '{2}'", o.GetType().Name, name, value);

			PropertyInfo propInfo;

			if (!TryGetPropertyInfo(o, name, out propInfo))
			{
				throw new NotSupportedException("Parameter " + name + " not supported on " + o.GetType().Name);
			}

			try
			{
				if (propInfo.IsDefined(typeof(ArrayParameterAttribute), false))
				{
					throw new NotSupportedException("Parameter " + name + " of " + o.GetType().Name + " is an array and cannot be assigned a scalar value.");
				}

				object newValue;

				Type propertyType = propInfo.PropertyType;

				propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

				if (!TryNLogSpecificConversion(propertyType, value, out newValue, cfg))
				{
					if (!TryGetEnumValue(propertyType, value, out newValue))
					{
						if (!TryImplicitConversion(propertyType, value, out newValue))
						{
							if (!TrySpecialConversion(propertyType, value, out newValue))
							{
								newValue = Convert.ChangeType(value, propertyType, CultureInfo.InvariantCulture);
							}
						}
					}
				}

				propInfo.SetValue(o, newValue, null);
			}
			catch (TargetInvocationException ex)
			{
				throw new NLogConfigurationException("Error when setting property '" + propInfo.Name + "' on " + o, ex.InnerException);
			}
			catch (Exception exception)
			{
				if (exception.MustBeRethrown())
				{
					throw;
				}

				throw new NLogConfigurationException("Error when setting property '" + propInfo.Name + "' on " + o, exception);
			}
		}

		internal static bool TryGetPropertyInfo(object o, string propertyName, out PropertyInfo result)
		{
			PropertyInfo propInfo = o.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
			if (propInfo != null)
			{
				result = propInfo;
				return true;
			}

			lock (parameterInfoCache)
			{
				Type targetType = o.GetType();
				Dictionary<string, PropertyInfo> cache;

				if (!parameterInfoCache.TryGetValue(targetType, out cache))
				{
					cache = BuildPropertyInfoDictionary(targetType);
					parameterInfoCache[targetType] = cache;
				}

				return cache.TryGetValue(propertyName, out result);
			}
		}

		internal static Type GetArrayItemType(PropertyInfo propInfo)
		{
			var arrayParameterAttribute = (ArrayParameterAttribute)Attribute.GetCustomAttribute(propInfo, typeof(ArrayParameterAttribute));
			if (arrayParameterAttribute != null)
			{
				return arrayParameterAttribute.ItemType;
			}

			return null;
		}

		private static bool TryImplicitConversion(Type resultType, string value, out object result)
		{
			MethodInfo operatorImplicitMethod = resultType.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
			if (operatorImplicitMethod == null)
			{
				result = null;
				return false;
			}

			result = operatorImplicitMethod.Invoke(null, new object[] { value });
			return true;
		}

		private static bool TryNLogSpecificConversion(Type propertyType, string value, out object newValue, LoggingConfiguration cfg)
		{
			if (propertyType == typeof(Layout) || propertyType == typeof(SimpleLayout))
			{
				newValue = new SimpleLayout(value, cfg);
				return true;
			}

			if (propertyType == typeof(ConditionExpression))
			{
				newValue = ConditionParser.ParseExpression(value, cfg);
				return true;
			}

			newValue = null;
			return false;
		}

		private static bool TryGetEnumValue(Type resultType, string value, out object result)
		{
			if (!resultType.IsEnum)
			{
				result = null;
				return false;
			}

			if (resultType.IsDefined(typeof(FlagsAttribute), false))
			{
				ulong union = 0;

				foreach (string v in value.Split(','))
				{
					FieldInfo enumField = resultType.GetField(v.Trim(), BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
					if (enumField == null)
					{
						throw new NLogConfigurationException("Invalid enumeration value '" + value + "'.");
					}

					union |= Convert.ToUInt64(enumField.GetValue(null), CultureInfo.InvariantCulture);
				}

				result = Convert.ChangeType(union, Enum.GetUnderlyingType(resultType), CultureInfo.InvariantCulture);
				result = Enum.ToObject(resultType, result);

				return true;
			}
			else
			{
				FieldInfo enumField = resultType.GetField(value, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
				if (enumField == null)
				{
					throw new NLogConfigurationException("Invalid enumeration value '" + value + "'.");
				}

				result = enumField.GetValue(null);
				return true;
			}
		}

		private static bool TrySpecialConversion(Type type, string value, out object newValue)
		{
			if (type == typeof(Uri))
			{
				newValue = new Uri(value, UriKind.RelativeOrAbsolute);
				return true;
			}

			if (type == typeof(Encoding))
			{
				newValue = Encoding.GetEncoding(value);
				return true;
			}

			if (type == typeof(CultureInfo))
			{
				newValue = new CultureInfo(value);
				return true;
			}

			if (type == typeof(Type))
			{
				newValue = Type.GetType(value, true);
				return true;
			}

			newValue = null;
			return false;
		}

		private static bool TryGetPropertyInfo(Type targetType, string propertyName, out PropertyInfo result)
		{
			if (!string.IsNullOrEmpty(propertyName))
			{
				PropertyInfo propInfo = targetType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
				if (propInfo != null)
				{
					result = propInfo;
					return true;
				}
			}

			lock (parameterInfoCache)
			{
				Dictionary<string, PropertyInfo> cache;

				if (!parameterInfoCache.TryGetValue(targetType, out cache))
				{
					cache = BuildPropertyInfoDictionary(targetType);
					parameterInfoCache[targetType] = cache;
				}

				return cache.TryGetValue(propertyName, out result);
			}
		}

		private static Dictionary<string, PropertyInfo> BuildPropertyInfoDictionary(Type t)
		{
			var retVal = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
			foreach (PropertyInfo propInfo in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				var arrayParameterAttribute = (ArrayParameterAttribute)Attribute.GetCustomAttribute(propInfo, typeof(ArrayParameterAttribute));

				if (arrayParameterAttribute != null)
				{
					retVal[arrayParameterAttribute.ElementName] = propInfo;
				}
				else
				{
					retVal[propInfo.Name] = propInfo;
				}

				if (propInfo.IsDefined(typeof(DefaultParameterAttribute), false))
				{
					// define a property with empty name
					retVal[string.Empty] = propInfo;
				}
			}

			return retVal;
		}
	}
}
