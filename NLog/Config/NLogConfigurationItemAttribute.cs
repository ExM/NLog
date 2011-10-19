
namespace NLog.Config
{
	using System;

	/// <summary>
	/// Marks the object as configuration item for NLog.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class NLogConfigurationItemAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NLogConfigurationItemAttribute"/> class.
		/// </summary>
		public NLogConfigurationItemAttribute()
		{
		}
	}
}
