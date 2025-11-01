# ![DeterministicGuids](https://raw.githubusercontent.com/MarkCiliaVincenti/DeterministicGuids/master/logo32.png)&nbsp;DeterministicGuids
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/MarkCiliaVincenti/DeterministicGuids/dotnet.yml?branch=master&logo=github&style=flat)](https://actions-badge.atrox.dev/MarkCiliaVincenti/DeterministicGuids/goto?ref=master) [![NuGet](https://img.shields.io/nuget/v/DeterministicGuids?label=NuGet&logo=nuget&style=flat)](https://www.nuget.org/packages/DeterministicGuids) [![NuGet](https://img.shields.io/nuget/dt/DeterministicGuids?logo=nuget&style=flat)](https://www.nuget.org/packages/DeterministicGuids) [![Codacy Grade](https://img.shields.io/codacy/grade/d49ddf9069194a939b7f690b6a95199e?style=flat)](https://app.codacy.com/gh/MarkCiliaVincenti/DeterministicGuids/dashboard) [![Codecov](https://img.shields.io/codecov/c/github/MarkCiliaVincenti/DeterministicGuids?label=coverage&logo=codecov&style=flat)](https://app.codecov.io/gh/MarkCiliaVincenti/DeterministicGuids)

A small, allocation-conscious, thread-safe .NET utility for generating **name-based deterministic UUIDs** (a.k.a. GUIDs) using RFC 4962 v3 (MD5), v5 (SHA-1) and v8 (SHA-256).

You give it:
- a *namespace GUID* (for a logical domain like "Orders", "Users", "Events")
- a *name* (string within that namespace)
- and (optionally) the UUID *version* (3, 5 or 8).  
  If you don't specify it, it defaults to version **5** (SHA-1). v8 is implemented using SHA-256.

It will always return the same GUID for the same `(namespace, name, version)` triplet.

This is useful for:
- Stable IDs across services or deployments
- Idempotent commands / events
- Importing external data but keeping predictable identifiers
- Deriving IDs from business keys without storing a lookup table

## Features
- ✅ **Deterministic**  
  Same `(namespace, name, version)` → same output every time.

- ✅ **Standards-based**  
  Implements RFC 4962 §5.3 for UUIDv3 (MD5-based), RFC 4962 §5.5 UUIDv5 (SHA-1-based) and RFC 4962 §5.8 for UUIDv8 (SHA-256-based).

- ✅ **Thread-safe**  
  No shared mutable state, no static caches, no locks. You can call it from many threads at once.

- ✅ **Optimized memory profile**  
  - Uses `Span<byte>` and `stackalloc` for fixed-size work.
  - Uses `ArrayPool<byte>` for variable-length buffers (the UTF-8 input string), instead of allocating on every call.
  - Uses incremental hashing on modern runtimes, so we never build a giant `namespace+name` concatenated array.

- ✅ **Zero runtime GUID parsing for known namespaces**  
  Namespaces are defined using the numeric `Guid` constructor, not `Guid.Parse(...)`, so there is no string parsing cost at first use.

- ✅ **Multi-targeted**  
  - `netstandard2.0`
  - `netstandard2.1`
  - `net5.0`
  - `net6.0`
  - `net7.0`
  - `net8.0` (and above)

On newer targets we take advantage of newer BCL APIs to reduce allocations to zero.

## Quick Start
```csharp
using DeterministicGuids;

// Choose a namespace (either one of the built-ins or your own Guid)
var ns = DeterministicGuid.Namespaces.Events;

// Define the stable name/key within that namespace
var key = "order:12345";

// Generate a deterministic GUID.
// This uses version 5 (SHA-1) by default.
Guid idV5 = DeterministicGuid.Create(ns, key);

Console.WriteLine(idV5);
// Same ns + key + version (5) will always produce the same Guid
```

## Benchmarks
DeterministicGuids stacks very well against other libraries in terms of speed, whilst being allocation-free. Here are some benchmarks with values taken directly from a run in GitHub Actions.

  DefaultJob : .NET 9.0.10 (9.0.10, 9.0.1025.47515), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.005 us | 0.0008 us | 0.0007 us |  1.00 |    0.00 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.533 us | 0.0110 us | 0.0103 us |  1.53 |    0.01 | 0.0744 |    1272 B |
| UUIDNext                      | 1.051 us | 0.0023 us | 0.0019 us |  1.05 |    0.00 |      - |         - |
| NGuid                         | 1.166 us | 0.0016 us | 0.0013 us |  1.16 |    0.00 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.671 us | 0.0154 us | 0.0136 us |  1.66 |    0.01 | 0.0763 |    1304 B |
| Enbrea.GuidFactory            | 1.601 us | 0.0072 us | 0.0064 us |  1.59 |    0.01 | 0.0763 |    1304 B |
| GuidPhantom                   | 1.520 us | 0.0053 us | 0.0047 us |  1.51 |    0.00 | 0.0744 |    1272 B |
| unique                        | 1.883 us | 0.0373 us | 0.0745 us |  1.87 |    0.07 | 0.0954 |    1600 B |

  DefaultJob : .NET 8.0.21 (8.0.21, 8.0.2125.47513), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.026 us | 0.0015 us | 0.0013 us |  1.00 |    0.00 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.531 us | 0.0084 us | 0.0075 us |  1.49 |    0.01 | 0.0744 |    1264 B |
| UUIDNext                      | 1.050 us | 0.0022 us | 0.0020 us |  1.02 |    0.00 |      - |         - |
| NGuid                         | 1.160 us | 0.0016 us | 0.0014 us |  1.13 |    0.00 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.689 us | 0.0080 us | 0.0074 us |  1.65 |    0.01 | 0.0763 |    1296 B |
| Enbrea.GuidFactory            | 1.625 us | 0.0040 us | 0.0036 us |  1.58 |    0.00 | 0.0763 |    1296 B |
| GuidPhantom                   | 1.538 us | 0.0069 us | 0.0064 us |  1.50 |    0.01 | 0.0744 |    1264 B |
| unique                        | 1.840 us | 0.0336 us | 0.0298 us |  1.79 |    0.03 | 0.0935 |    1592 B |

  DefaultJob : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|-------:|----------:|
| DeterministicGuids            | 1.067 us | 0.0015 us | 0.0014 us |  1.00 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.564 us | 0.0026 us | 0.0020 us |  1.47 | 0.0744 |    1264 B |
| UUIDNext                      | 1.102 us | 0.0011 us | 0.0011 us |  1.03 |      - |         - |
| NGuid                         | 1.184 us | 0.0013 us | 0.0012 us |  1.11 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.735 us | 0.0052 us | 0.0049 us |  1.63 | 0.0763 |    1296 B |
| Enbrea.GuidFactory            | 1.650 us | 0.0053 us | 0.0049 us |  1.55 | 0.0763 |    1296 B |
| GuidPhantom                   | 1.561 us | 0.0038 us | 0.0032 us |  1.46 | 0.0744 |    1264 B |
| unique                        | 1.956 us | 0.0039 us | 0.0034 us |  1.83 | 0.0916 |    1592 B |

  DefaultJob : .NET Core 3.1.32 (3.1.32, 4.700.22.55902), X64 RyuJIT VectorSize=256
| Method                   | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated |
|------------------------- |---------:|----------:|----------:|------:|-------:|----------:|
| DeterministicGuids       | 1.115 us | 0.0052 us | 0.0043 us |  1.00 |      - |         - |
| UUIDNext                 | 1.150 us | 0.0042 us | 0.0037 us |  1.03 |      - |         - |
| NGuid                    | 1.868 us | 0.0072 us | 0.0068 us |  1.67 | 0.0820 |    1376 B |
| Elephant.Uuidv5Utilities | 1.874 us | 0.0062 us | 0.0058 us |  1.68 | 0.0763 |    1296 B |
| GuidPhantom              | 1.692 us | 0.0064 us | 0.0057 us |  1.52 | 0.0744 |    1264 B |
| unique                   | 2.430 us | 0.0047 us | 0.0044 us |  2.18 | 0.0916 |    1592 B |

  DefaultJob : .NET Framework 4.8.1 (4.8.9310.0), X64 RyuJIT VectorSize=256
| Method                   | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| DeterministicGuids       |  2.832 us | 0.0060 us | 0.0053 us |  1.00 |    0.00 | 0.0992 |     642 B |        1.00 |
| UUIDNext                 |  2.506 us | 0.0074 us | 0.0065 us |  0.88 |    0.00 | 0.1106 |     698 B |        1.09 |
| NGuid                    | 10.059 us | 0.1256 us | 0.0980 us |  3.55 |    0.03 | 0.4578 |    2937 B |        4.57 |
| Elephant.Uuidv5Utilities |  3.611 us | 0.0408 us | 0.0341 us |  1.27 |    0.01 | 0.2403 |    1533 B |        2.39 |
| GuidPhantom              |  4.013 us | 0.0175 us | 0.0155 us |  1.42 |    0.01 | 0.2441 |    1581 B |        2.46 |
| unique                   |  5.279 us | 0.0145 us | 0.0121 us |  1.86 |    0.01 | 0.2975 |    1910 B |        2.98 |

## Credits
Check out our [list of contributors](https://github.com/MarkCiliaVincenti/DeterministicGuids/blob/master/CONTRIBUTORS.md)!
