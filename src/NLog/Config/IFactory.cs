
namespace NLog.Config
{
	using System;
	using System.Reflection;

	/// <summary>
	/// Provides means to populate factories of named items (such as targets, layouts, layout renderers, etc.).
	/// </summary>
	internal interface IFactory
	{
		void Clear();

		void ScanAssembly(Assembly theAssembly, string prefix);

		void RegisterType(Type type, string itemNamePrefix);
	}
}
