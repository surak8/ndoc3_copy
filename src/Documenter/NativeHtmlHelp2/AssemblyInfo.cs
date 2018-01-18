using System;
using System.Reflection;

[assembly: CLSCompliantAttribute(true)]
[assembly: AssemblyTitle("NDoc3 Native HTML Help 2 Documenter")]
[assembly: AssemblyDescription("Native HTML Help 2.0 documenter for the NDoc3 code documentation generator.")]

#if (OFFICIAL_RELEASE)
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("NDoc3.snk")]
[assembly: AssemblyKeyName("")]
#endif
