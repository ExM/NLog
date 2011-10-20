
namespace NLog.Config
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using NLog.Conditions;
	using NLog.Filters;
	using NLog.Internal;
	using NLog.LayoutRenderers;
	using NLog.Layouts;
	using NLog.Targets;

	/// <summary>
	/// Constructs a new instance the configuration item (target, layout, layout renderer, etc.) given its type.
	/// </summary>
	/// <param name="itemType">Type of the item.</param>
	/// <returns>Created object of the specified type.</returns>
	public delegate object ConfigurationItemCreator(Type itemType);
}
