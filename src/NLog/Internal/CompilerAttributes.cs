
#if NET2_0 || NETCF2_0

namespace System.Runtime.CompilerServices
{
	using System;

	/// <summary>
	/// Extension method attribute used when compiling for pre-LINQ platforms.
	/// </summary>
	internal class ExtensionAttribute : Attribute
	{
	}
}

#endif