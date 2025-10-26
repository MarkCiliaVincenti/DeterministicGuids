using Be.Vlaanderen.Basisregisters.Generators.Guid;
using BenchmarkDotNet.Attributes;
using DeterministicGuids;
using UUIDNext;

namespace Benchmarks;

[MemoryDiagnoser]
public class Benchmarks
{
    private const string name = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
    private static readonly Guid ns = Guid.NewGuid();

    [Benchmark(Baseline = true)]
    public void DeterministicGuids()
    {
        Parallel.ForEach(Enumerable.Range(0, 1000), _ =>
        {
            var guid = DeterministicGuid.Create(ns, name);
        }); 
    }

    [Benchmark]
    public void Be_Vlaanderen_Basisregisters_Generators_Guid_Deterministic()
    {
        Parallel.ForEach(Enumerable.Range(0, 1000), _ =>
        {
            var guid = Deterministic.Create(ns, name);
        });
    }

    [Benchmark]
    public void UUIDNext()
    {
        Parallel.ForEach(Enumerable.Range(0, 1000), _ =>
        {
            var guid = Uuid.NewNameBased(ns, name);
        });
    }
}