
namespace MyExtensionNamespace
{
	using System;

	using NLog.Conditions;

	[ConditionMethods]
	public static class MyConditionMethods
	{
		[ConditionMethod("myrandom")]
		public static int Random(int max)
		{
			return new Random().Next(max);
		}
	}
}