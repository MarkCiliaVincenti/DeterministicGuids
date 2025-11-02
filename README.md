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
| DeterministicGuids            | 1.019 us | 0.0069 us | 0.0061 us |  1.00 |    0.01 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.533 us | 0.0262 us | 0.0269 us |  1.50 |    0.03 | 0.0744 |    1272 B |
| UUIDNext                      | 1.040 us | 0.0068 us | 0.0061 us |  1.02 |    0.01 |      - |         - |
| NGuid                         | 1.175 us | 0.0091 us | 0.0085 us |  1.15 |    0.01 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.661 us | 0.0178 us | 0.0139 us |  1.63 |    0.02 | 0.0763 |    1304 B |
| Enbrea.GuidFactory            | 1.604 us | 0.0074 us | 0.0062 us |  1.57 |    0.01 | 0.0763 |    1304 B |
| GuidPhantom                   | 1.534 us | 0.0234 us | 0.0207 us |  1.51 |    0.02 | 0.0744 |    1272 B |
| unique                        | 1.810 us | 0.0103 us | 0.0096 us |  1.78 |    0.01 | 0.0954 |    1600 B |

  DefaultJob : .NET 8.0.21 (8.0.21, 8.0.2125.47513), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|-------:|----------:|
| DeterministicGuids            | 1.031 us | 0.0062 us | 0.0052 us |  1.00 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.544 us | 0.0122 us | 0.0114 us |  1.50 | 0.0744 |    1264 B |
| UUIDNext                      | 1.049 us | 0.0068 us | 0.0063 us |  1.02 |      - |         - |
| NGuid                         | 1.158 us | 0.0006 us | 0.0005 us |  1.12 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.709 us | 0.0115 us | 0.0107 us |  1.66 | 0.0763 |    1296 B |
| Enbrea.GuidFactory            | 1.623 us | 0.0060 us | 0.0050 us |  1.57 | 0.0763 |    1296 B |
| GuidPhantom                   | 1.556 us | 0.0124 us | 0.0116 us |  1.51 | 0.0744 |    1264 B |
| unique                        | 1.847 us | 0.0084 us | 0.0066 us |  1.79 | 0.0935 |    1592 B |

  DefaultJob : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|-------:|----------:|
| DeterministicGuids            | 1.058 us | 0.0030 us | 0.0025 us |  1.00 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.546 us | 0.0093 us | 0.0087 us |  1.46 | 0.0744 |    1264 B |
| UUIDNext                      | 1.095 us | 0.0015 us | 0.0013 us |  1.03 |      - |         - |
| NGuid                         | 1.199 us | 0.0109 us | 0.0091 us |  1.13 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.738 us | 0.0052 us | 0.0049 us |  1.64 | 0.0763 |    1296 B |
| Enbrea.GuidFactory            | 1.670 us | 0.0061 us | 0.0051 us |  1.58 | 0.0763 |    1296 B |
| GuidPhantom                   | 1.547 us | 0.0070 us | 0.0065 us |  1.46 | 0.0744 |    1264 B |
| unique                        | 1.981 us | 0.0138 us | 0.0129 us |  1.87 | 0.0916 |    1592 B |

  DefaultJob : .NET 5.0.17 (5.0.17, 5.0.1722.21314), X64 RyuJIT x86-64-v3
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids       | 1.104 us | 0.0199 us | 0.0186 us |  1.00 |    0.02 |      - |         - |
| UUIDNext                 | 1.148 us | 0.0014 us | 0.0012 us |  1.04 |    0.02 |      - |         - |
| NGuid                    | 1.760 us | 0.0180 us | 0.0160 us |  1.59 |    0.03 | 0.0820 |    1376 B |
| Elephant.Uuidv5Utilities | 1.766 us | 0.0119 us | 0.0111 us |  1.60 |    0.03 | 0.0763 |    1296 B |
| GuidPhantom              | 1.612 us | 0.0228 us | 0.0213 us |  1.46 |    0.03 | 0.0744 |    1264 B |
| unique                   | 1.983 us | 0.0336 us | 0.0298 us |  1.80 |    0.04 | 0.0916 |    1592 B |

  DefaultJob : .NET Core 3.1.32 (3.1.32, 4.700.22.55902), X64 RyuJIT VectorSize=256
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids       | 1.121 us | 0.0121 us | 0.0094 us |  1.00 |    0.01 |      - |         - |
| UUIDNext                 | 1.169 us | 0.0160 us | 0.0142 us |  1.04 |    0.01 |      - |         - |
| NGuid                    | 1.869 us | 0.0362 us | 0.0355 us |  1.67 |    0.03 | 0.0820 |    1376 B |
| Elephant.Uuidv5Utilities | 1.899 us | 0.0254 us | 0.0225 us |  1.69 |    0.02 | 0.0763 |    1296 B |
| GuidPhantom              | 1.719 us | 0.0257 us | 0.0241 us |  1.53 |    0.02 | 0.0744 |    1264 B |
| unique                   | 2.424 us | 0.0059 us | 0.0052 us |  2.16 |    0.02 | 0.0916 |    1592 B |

  DefaultJob : .NET Framework 4.8.1 (4.8.9310.0), X64 RyuJIT VectorSize=256
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| DeterministicGuids       | 2.701 us | 0.0106 us | 0.0094 us |  1.00 |    0.00 | 0.0305 |     209 B |        1.00 |
| UUIDNext                 | 2.525 us | 0.0139 us | 0.0123 us |  0.93 |    0.01 | 0.1106 |     698 B |        3.34 |
| NGuid                    | 9.984 us | 0.0768 us | 0.0719 us |  3.70 |    0.03 | 0.4578 |    2937 B |       14.05 |
| Elephant.Uuidv5Utilities | 3.529 us | 0.0189 us | 0.0167 us |  1.31 |    0.01 | 0.2403 |    1533 B |        7.33 |
| GuidPhantom              | 3.980 us | 0.0261 us | 0.0218 us |  1.47 |    0.01 | 0.2441 |    1581 B |        7.56 |
| unique                   | 5.125 us | 0.0229 us | 0.0203 us |  1.90 |    0.01 | 0.2975 |    1910 B |        9.14 |

## Credits
Check out our [list of contributors](https://github.com/MarkCiliaVincenti/DeterministicGuids/blob/master/CONTRIBUTORS.md)!
