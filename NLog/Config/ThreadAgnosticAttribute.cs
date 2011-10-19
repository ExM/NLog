
namespace NLog.Config
{
	using System;

	/// <summary>
	/// Marks the layout or layout renderer as producing correct results regardless of the thread
	/// it's running on.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ThreadAgnosticAttribute : Attribute
	{
	}
}
