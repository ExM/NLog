using System;
using System.Collections.Generic;
using System.Reflection;
using NLog.Common;
using NLog.Internal;

namespace NLog.Config
{
	/// <summary>
	/// Factory for class-based items.
	/// </summary>
	/// <typeparam name="TBaseType">The base type of each item.</typeparam>
	/// <typeparam name="TAttributeType">The type of the attribute used to annotate itemss.</typeparam>
	internal class Factory<TBaseType, TAttributeType> : INamedItemFactory<TBaseType, Type>, IFactory
		where TBaseType : class 
		where TAttributeType : NameBaseAttribute
	{
		private readonly Dictionary<string, GetTypeDelegate> _items = new Dictionary<string, GetTypeDelegate>(StringComparer.OrdinalIgnoreCase);
		private ConfigurationItemFactory _parentFactory;

		internal Factory(ConfigurationItemFactory parentFactory)
		{
			_parentFactory = parentFactory;
		}

		private delegate Type GetTypeDelegate();

		/// <summary>
		/// Scans the assembly.
		/// </summary>
		/// <param name="theAssembly">The assembly.</param>
		/// <param name="prefix">The prefix.</param>
		public void ScanAssembly(Assembly theAssembly, string prefix)
		{
			try
			{
				InternalLogger.Debug("ScanAssembly('{0}','{1}','{2}')", theAssembly.FullName, typeof(TAttributeType), typeof(TBaseType));
				foreach (Type t in theAssembly.SafeGetTypes())
					RegisterType(t, prefix);
			}
			catch (Exception exception)
			{
				if (exception.MustBeRethrown())
					throw;

				InternalLogger.Error("Failed to add targets from '" + theAssembly.FullName + "': {0}", exception);
			}
		}

		/// <summary>
		/// Registers the type.
		/// </summary>
		/// <param name="type">The type to register.</param>
		/// <param name="itemNamePrefix">The item name prefix.</param>
		public void RegisterType(Type type, string itemNamePrefix)
		{
			TAttributeType[] attributes = (TAttributeType[])type.GetCustomAttributes(typeof(TAttributeType), false);
			if (attributes == null)
				return;

			foreach (TAttributeType attr in attributes)
				RegisterDefinition(itemNamePrefix + attr.Name, type);
		}

		/// <summary>
		/// Registers the item based on a type name.
		/// </summary>
		/// <param name="itemName">Name of the item.</param>
		/// <param name="typeName">Name of the type.</param>
		public void RegisterNamedType(string itemName, string typeName)
		{
			_items[itemName] = () => Type.GetType(typeName, false);
		}

		/// <summary>
		/// Clears the contents of the factory.
		/// </summary>
		public void Clear()
		{
			_items.Clear();
		}

		/// <summary>
		/// Registers a single type definition.
		/// </summary>
		/// <param name="name">The item name.</param>
		/// <param name="type">The type of the item.</param>
		public void RegisterDefinition(string name, Type type)
		{
			_items[name] = () => type;
		}

		/// <summary>
		/// Tries to get registed item definition.
		/// </summary>
		/// <param name="itemName">Name of the item.</param>
		/// <param name="result">Reference to a variable which will store the item definition.</param>
		/// <returns>Item definition.</returns>
		public bool TryGetDefinition(string itemName, out Type result)
		{
			GetTypeDelegate del;

			if (!_items.TryGetValue(itemName, out del))
			{
				result = null;
				return false;
			}

			try
			{
				result = del();
				return result != null;
			}
			catch (Exception ex)
			{
				if (ex.MustBeRethrown())
					throw;

				// delegate invocation failed - type is not available
				result = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to create an item instance.
		/// </summary>
		/// <param name="itemName">Name of the item.</param>
		/// <param name="result">The result.</param>
		/// <returns>True if instance was created successfully, false otherwise.</returns>
		public bool TryCreateInstance(string itemName, out TBaseType result)
		{
			Type type;

			if (!TryGetDefinition(itemName, out type))
			{
				result = null;
				return false;
			}

			result = (TBaseType)_parentFactory.CreateInstance(type);
			return true;
		}

		/// <summary>
		/// Creates an item instance.
		/// </summary>
		/// <param name="name">The name of the item.</param>
		/// <returns>Created item.</returns>
		public TBaseType CreateInstance(string name)
		{
			TBaseType result;

			if (TryCreateInstance(name, out result))
				return result;

			throw new ArgumentException(typeof(TBaseType).Name + " cannot be found: '" + name + "'");
		}
	}
}
