
namespace NLog.Conditions
{
	using System;

	/// <summary>
	/// Marks the class as containing condition methods.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class ConditionMethodsAttribute : Attribute
	{
	}
}
