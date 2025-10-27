using Be.Vlaanderen.Basisregisters.Generators.Guid;
using BenchmarkDotNet.Attributes;
using DeterministicGuids;
using Elephant.Uuidv5Utilities;
using Enbrea.GuidFactory;
using GuidPhantom;
using NGuid;
using Unique.CSharp;
using UUIDNext;

namespace Benchmarks;

[MemoryDiagnoser]
public class Benchmarks
{
    private const string name = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
    private static readonly Guid ns = Guid.NewGuid();

    [Benchmark(Baseline = true)]
    public Guid DeterministicGuids() =>
        DeterministicGuid.Create(ns, name);

    [Benchmark(Description = "Be.Vlaanderen.Basisregisters.Generators.Guid.Deterministic")]
    public Guid Be_Vlaanderen_Basisregisters_Generators_Guid_Deterministic() =>
        Deterministic.Create(ns, name);

    [Benchmark]
    public Guid UUIDNext() =>
        Uuid.NewNameBased(ns, name);

    [Benchmark]
    public Guid NGuid() =>
        GuidHelpers.CreateFromName(ns, name);

    [Benchmark(Description = "Elephant.Uuidv5Utilities")]
    public Guid Elephant_Uuidv5Utilities() =>
        Uuidv5Utils.GenerateGuid(ns, name);

    [Benchmark(Description = "Enbrea.GuidFactory")]
    public Guid Enbrea_GuidFactory() =>
        GuidGenerator.Create(ns, name);

    [Benchmark]
    public Guid GuidPhantom() =>
        GuidKit.CreateVersion5(ns, name);

    [Benchmark(Description = "unique")]
    public Guid Unique() =>
        NamedGuid.NewGuid(ns, name);
}