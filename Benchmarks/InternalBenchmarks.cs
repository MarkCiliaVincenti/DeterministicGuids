using BenchmarkDotNet.Attributes;
using DeterministicGuids;

namespace Benchmarks;

/// <summary>
/// Benchmarks covering all three UUID versions (v3/MD5, v5/SHA-1, v8/SHA-256)
/// and two name lengths (short stackalloc path, long heap-fallback path).
/// Useful for regression detection across code paths within DeterministicGuids itself.
/// </summary>
[MemoryDiagnoser]
public class InternalBenchmarks
{
    // Short: well under the stackalloc threshold (10 bytes UTF-8)
    private const string ShortName = "python.org";

    // Long: exceeds the 496-byte stackalloc threshold on NET8+ (512 on NET5-7),
    // exercises the heap-allocation fallback path
    private const string LongName =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut " +
        "labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco " +
        "laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in " +
        "voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat " +
        "non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. Sed ut perspiciatis " +
        "unde omnis iste natus error sit voluptatem accusantium doloremque laudantium.";

    private static readonly Guid Ns = DeterministicGuid.Namespaces.Dns;

    [Params(DeterministicGuid.Version.MD5, DeterministicGuid.Version.SHA1, DeterministicGuid.Version.SHA256)]
    public DeterministicGuid.Version Version { get; set; }

    [Benchmark(Baseline = true)]
    public Guid Short_Name() => DeterministicGuid.Create(Ns, ShortName, Version);

    [Benchmark]
    public Guid Long_Name() => DeterministicGuid.Create(Ns, LongName, Version);
}
