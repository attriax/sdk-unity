#nullable enable
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

[assembly: Obfuscation(Exclude = true, ApplyToMembers = true, StripAfterObfuscation = false)]
[assembly: InternalsVisibleTo("Attriax.EditorTests")]
[assembly: AlwaysLinkAssembly]