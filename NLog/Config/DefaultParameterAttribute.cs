using System;

namespace NLog.Config
{
	/// <summary>
	/// Attribute used to mark the default parameters for layout renderers.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class DefaultParameterAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParameterAttribute" /> class.
		/// </summary>
		public DefaultParameterAttribute()
		{
		}
	}
}
