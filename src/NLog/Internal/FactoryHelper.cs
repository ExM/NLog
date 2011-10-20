
namespace NLog.Internal
{
	using System;
	using System.Reflection;
	using NLog.Config;

	/// <summary>
	/// Object construction helper.
	/// </summary>
	internal class FactoryHelper
	{
		private static Type[] emptyTypes = new Type[0];
		private static object[] emptyParams = new object[0];

		private FactoryHelper()
		{
		}

		internal static object CreateInstance(Type t)
		{
			ConstructorInfo constructor = t.GetConstructor(emptyTypes);
			if (constructor != null)
			{
				return constructor.Invoke(emptyParams);
			}
			else
			{
				throw new NLogConfigurationException("Cannot access the constructor of type: " + t.FullName + ". Is the required permission granted?");
			}
		}
	}
}
