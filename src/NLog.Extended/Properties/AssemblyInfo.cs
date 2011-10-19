
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET2_0
	[assembly: AssemblyTitle("NLog for .NET Framework 2.0 Extended Profile")]
#elif NET3_5
	[assembly: AssemblyTitle("NLog for .NET Framework 3.5 Extended Profile")]
#elif NET4_0
	[assembly: AssemblyTitle("NLog for .NET Framework 4 Extended Profile")]
#elif MONO_2_0
	[assembly: AssemblyTitle("NLog for Mono 2.0 Extended Profile")]
#elif DOCUMENTATION
	[assembly: AssemblyTitle("NLog Documentation")]
#else
#error Unrecognized build target - please update AssemblyInfo.cs
#endif

[assembly: AssemblyDescription("NLog")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("NLog")]
[assembly: AssemblyCopyright("Copyright (c) 2004-2011 by Jaroslaw Kowalski")]
[assembly: AssemblyCulture("")]

[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]

[assembly: InternalsVisibleTo("NLog.UnitTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100ef8eab4fbdeb511eeb475e1659fe53f00ec1c1340700f1aa347bf3438455d71993b28b1efbed44c8d97a989e0cb6f01bcb5e78f0b055d311546f63de0a969e04cf04450f43834db9f909e566545a67e42822036860075a1576e90e1c43d43e023a24c22a427f85592ae56cac26f13b7ec2625cbc01f9490d60f16cfbb1bc34d9")]
