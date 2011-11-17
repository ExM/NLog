using System;
using NLog.LayoutRenderers;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.Config
{
	/// <summary>
	/// Attaches a simple name to an item (such as <see cref="Target"/>, 
	/// <see cref="LayoutRenderer"/>, <see cref="Layout"/>, etc.).
	/// </summary>
	public abstract class NameBaseAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NameBaseAttribute" /> class.
		/// </summary>
		/// <param name="name">The name of the item.</param>
		protected NameBaseAttribute(string name)
		{
			this.Name = name;
		}

		/// <summary>
		/// Gets the name of the item.
		/// </summary>
		/// <value>The name of the item.</value>
		public string Name { get; private set; }
	}
}
