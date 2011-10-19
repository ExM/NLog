
namespace NLog.Config
{
	using System;

	/// <summary>
	/// Marks the class or a member as advanced. Advanced classes and members are hidden by 
	/// default in generated documentation.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class AdvancedAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AdvancedAttribute" /> class.
		/// </summary>
		public AdvancedAttribute()
		{
		}
	}
}
