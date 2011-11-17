using System;

namespace NLog.Config
{
	/// <summary>
	/// Attribute used to mark the required parameters for targets,
	/// layout targets and filters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class RequiredParameterAttribute : Attribute
	{
	}
}
