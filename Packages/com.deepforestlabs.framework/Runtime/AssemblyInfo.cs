#nullable enable
using System.Runtime.CompilerServices;
using ZLinq;

[assembly: InternalsVisibleTo("DeepForestLabs.Editor")]
[assembly: InternalsVisibleTo("DeepForestLabs.Tests.Runtime")]
[assembly: InternalsVisibleTo("DeepForestLabs.Tests.Editor")]
[assembly: ZLinqDropIn("DeepForestLabs", DropInGenerateTypes.Array | DropInGenerateTypes.List)]
#nullable disable