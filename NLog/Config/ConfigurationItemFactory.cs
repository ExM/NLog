using System;
using System.Collections.Generic;
using System.Reflection;
using NLog.Conditions;
using NLog.Filters;
using NLog.Internal;
using NLog.LayoutRenderers;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.Config
{
	/// <summary>
	/// Provides registration information for named items (targets, layouts, layout renderers, etc.) managed by NLog.
	/// </summary>
	public class ConfigurationItemFactory
	{
		private readonly IList<object> _allFactories;
		private readonly Factory<Target, TargetAttribute> _targets;
		private readonly Factory<Filter, FilterAttribute> _filters;
		private readonly Factory<LayoutRenderer, LayoutRendererAttribute> _layoutRenderers;
		private readonly Factory<Layout, LayoutAttribute> _layouts;
		private readonly MethodFactory<ConditionMethodsAttribute, ConditionMethodAttribute> _conditionMethods;
		private readonly Factory<LayoutRenderer, AmbientPropertyAttribute> _ambientProperties;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationItemFactory"/> class.
		/// </summary>
		/// <param name="assemblies">The assemblies to scan for named items.</param>
		public ConfigurationItemFactory(params Assembly[] assemblies)
		{
			CreateInstance = FactoryHelper.CreateInstance;
			_targets = new Factory<Target, TargetAttribute>(this);
			_filters = new Factory<Filter, FilterAttribute>(this);
			_layoutRenderers = new Factory<LayoutRenderer, LayoutRendererAttribute>(this);
			_layouts = new Factory<Layout, LayoutAttribute>(this);
			_conditionMethods = new MethodFactory<ConditionMethodsAttribute, ConditionMethodAttribute>();
			_ambientProperties = new Factory<LayoutRenderer, AmbientPropertyAttribute>(this);
			_allFactories = new List<object>
			{
				_targets,
				_filters,
				_layoutRenderers,
				_layouts,
				_conditionMethods,
				_ambientProperties,
			};

			foreach (var asm in assemblies)
				RegisterItemsFromAssembly(asm);
		}

		/// <summary>
		/// Gets or sets the creator delegate used to instantiate configuration objects.
		/// </summary>
		/// <remarks>
		/// By overriding this property, one can enable dependency injection or interception for created objects.
		/// </remarks>
		public Func<Type, object> CreateInstance { get; set; }

		/// <summary>
		/// Gets the <see cref="Target"/> factory.
		/// </summary>
		/// <value>The target factory.</value>
		public INamedItemFactory<Target, Type> Targets
		{
			get
			{
				return _targets;
			}
		}

		/// <summary>
		/// Gets the <see cref="Filter"/> factory.
		/// </summary>
		/// <value>The filter factory.</value>
		public INamedItemFactory<Filter, Type> Filters
		{
			get
			{
				return _filters;
			}
		}

		/// <summary>
		/// Gets the <see cref="LayoutRenderer"/> factory.
		/// </summary>
		/// <value>The layout renderer factory.</value>
		public INamedItemFactory<LayoutRenderer, Type> LayoutRenderers
		{
			get
			{
				return _layoutRenderers;
			}
		}

		/// <summary>
		/// Gets the <see cref="LayoutRenderer"/> factory.
		/// </summary>
		/// <value>The layout factory.</value>
		public INamedItemFactory<Layout, Type> Layouts
		{
			get
			{
				return _layouts;
			}
		}

		/// <summary>
		/// Gets the ambient property factory.
		/// </summary>
		/// <value>The ambient property factory.</value>
		public INamedItemFactory<LayoutRenderer, Type> AmbientProperties
		{
			get
			{
				return _ambientProperties;
			}
		}

		/// <summary>
		/// Gets the condition method factory.
		/// </summary>
		/// <value>The condition method factory.</value>
		public INamedItemFactory<MethodInfo, MethodInfo> ConditionMethods
		{
			get
			{
				return _conditionMethods;
			}
		}

		/// <summary>
		/// Registers named items from the assembly.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		public void RegisterItemsFromAssembly(Assembly assembly)
		{
			RegisterItemsFromAssembly(assembly, string.Empty);
		}

		/// <summary>
		/// Registers named items from the assembly.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <param name="itemNamePrefix">Item name prefix.</param>
		public void RegisterItemsFromAssembly(Assembly assembly, string itemNamePrefix)
		{
			foreach (IFactory f in _allFactories)
				f.ScanAssembly(assembly, itemNamePrefix);
		}

		/// <summary>
		/// Clears the contents of all factories.
		/// </summary>
		public void Clear()
		{
			foreach (IFactory f in _allFactories)
				f.Clear();
		}

		/// <summary>
		/// Registers the type.
		/// </summary>
		/// <param name="type">The type to register.</param>
		/// <param name="itemNamePrefix">The item name prefix.</param>
		public void RegisterType(Type type, string itemNamePrefix)
		{
			foreach (IFactory f in _allFactories)
				f.RegisterType(type, itemNamePrefix);
		}
	}
}
