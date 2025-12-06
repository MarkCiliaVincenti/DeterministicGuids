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

  DefaultJob : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.015 us | 0.0079 us | 0.0070 us |  1.00 |    0.01 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.554 us | 0.0214 us | 0.0200 us |  1.53 |    0.02 | 0.0744 |    1272 B |
| UUIDNext                      | 1.038 us | 0.0025 us | 0.0022 us |  1.02 |    0.01 |      - |         - |
| NGuid                         | 1.150 us | 0.0164 us | 0.0154 us |  1.13 |    0.02 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.640 us | 0.0041 us | 0.0036 us |  1.62 |    0.01 | 0.0763 |    1304 B |
| Enbrea.GuidFactory            | 1.639 us | 0.0281 us | 0.0249 us |  1.62 |    0.03 | 0.0763 |    1304 B |
| GuidPhantom                   | 1.526 us | 0.0282 us | 0.0264 us |  1.50 |    0.03 | 0.0744 |    1272 B |
| unique                        | 1.813 us | 0.0346 us | 0.0425 us |  1.79 |    0.04 | 0.0916 |    1560 B |

  DefaultJob : .NET 9.0.11 (9.0.11, 9.0.1125.51716), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.037 us | 0.0138 us | 0.0129 us |  1.00 |    0.02 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.554 us | 0.0303 us | 0.0471 us |  1.50 |    0.05 | 0.0744 |    1272 B |
| UUIDNext                      | 1.122 us | 0.0222 us | 0.0332 us |  1.08 |    0.03 |      - |         - |
| NGuid                         | 1.265 us | 0.0153 us | 0.0143 us |  1.22 |    0.02 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.690 us | 0.0187 us | 0.0156 us |  1.63 |    0.02 | 0.0763 |    1304 B |
| Enbrea.GuidFactory            | 1.612 us | 0.0064 us | 0.0053 us |  1.55 |    0.02 | 0.0763 |    1304 B |
| GuidPhantom                   | 1.530 us | 0.0150 us | 0.0125 us |  1.48 |    0.02 | 0.0744 |    1272 B |
| unique                        | 1.881 us | 0.0296 us | 0.0563 us |  1.81 |    0.06 | 0.0954 |    1600 B |

  DefaultJob : .NET 8.0.22 (8.0.22, 8.0.2225.52707), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.059 us | 0.0206 us | 0.0193 us |  1.00 |    0.02 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.586 us | 0.0182 us | 0.0170 us |  1.50 |    0.03 | 0.0744 |    1264 B |
| UUIDNext                      | 1.057 us | 0.0095 us | 0.0079 us |  1.00 |    0.02 |      - |         - |
| NGuid                         | 1.175 us | 0.0205 us | 0.0192 us |  1.11 |    0.03 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.733 us | 0.0152 us | 0.0127 us |  1.64 |    0.03 | 0.0763 |    1296 B |
| Enbrea.GuidFactory            | 1.661 us | 0.0202 us | 0.0179 us |  1.57 |    0.03 | 0.0763 |    1296 B |
| GuidPhantom                   | 1.571 us | 0.0303 us | 0.0324 us |  1.48 |    0.04 | 0.0744 |    1264 B |
| unique                        | 1.854 us | 0.0339 us | 0.0301 us |  1.75 |    0.04 | 0.0916 |    1592 B |

 DefaultJob : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|-------:|----------:|
| DeterministicGuids            | 1.056 us | 0.0011 us | 0.0010 us |  1.00 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.588 us | 0.0092 us | 0.0081 us |  1.50 | 0.0744 |    1264 B |
| UUIDNext                      | 1.106 us | 0.0016 us | 0.0015 us |  1.05 |      - |         - |
| NGuid                         | 1.183 us | 0.0011 us | 0.0010 us |  1.12 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.736 us | 0.0078 us | 0.0073 us |  1.64 | 0.0763 |    1296 B |
| Enbrea.GuidFactory            | 1.649 us | 0.0057 us | 0.0047 us |  1.56 | 0.0763 |    1296 B |
| GuidPhantom                   | 1.579 us | 0.0060 us | 0.0053 us |  1.49 | 0.0744 |    1264 B |
| unique                        | 1.939 us | 0.0144 us | 0.0135 us |  1.84 | 0.0916 |    1592 B |

  DefaultJob : .NET 5.0.17 (5.0.17, 5.0.1722.21314), X64 RyuJIT x86-64-v3
| Method                   | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated |
|------------------------- |---------:|----------:|----------:|------:|-------:|----------:|
| DeterministicGuids       | 1.065 us | 0.0012 us | 0.0011 us |  1.00 |      - |         - |
| UUIDNext                 | 1.141 us | 0.0015 us | 0.0014 us |  1.07 |      - |         - |
| NGuid                    | 1.733 us | 0.0079 us | 0.0070 us |  1.63 | 0.0820 |    1376 B |
| Elephant.Uuidv5Utilities | 1.789 us | 0.0077 us | 0.0072 us |  1.68 | 0.0763 |    1296 B |
| GuidPhantom              | 1.577 us | 0.0052 us | 0.0046 us |  1.48 | 0.0744 |    1264 B |
| unique                   | 1.940 us | 0.0059 us | 0.0055 us |  1.82 | 0.0916 |    1592 B |

  DefaultJob : .NET Core 3.1.32 (3.1.32, 4.700.22.55902), X64 RyuJIT VectorSize=256
| Method                   | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated |
|------------------------- |---------:|----------:|----------:|------:|-------:|----------:|
| DeterministicGuids       | 1.120 us | 0.0007 us | 0.0006 us |  1.00 |      - |         - |
| UUIDNext                 | 1.156 us | 0.0012 us | 0.0010 us |  1.03 |      - |         - |
| NGuid                    | 1.872 us | 0.0053 us | 0.0044 us |  1.67 | 0.0820 |    1376 B |
| Elephant.Uuidv5Utilities | 1.881 us | 0.0094 us | 0.0088 us |  1.68 | 0.0763 |    1296 B |
| GuidPhantom              | 1.690 us | 0.0055 us | 0.0049 us |  1.51 | 0.0744 |    1264 B |
| unique                   | 2.437 us | 0.0077 us | 0.0068 us |  2.18 | 0.0916 |    1592 B |

  DefaultJob : .NET Framework 4.8.1 (4.8.9310.0), X64 RyuJIT VectorSize=256
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| DeterministicGuids       | 2.696 us | 0.0085 us | 0.0071 us |  1.00 |    0.00 | 0.0305 |     209 B |        1.00 |
| UUIDNext                 | 2.504 us | 0.0069 us | 0.0064 us |  0.93 |    0.00 | 0.1106 |     698 B |        3.34 |
| NGuid                    | 9.731 us | 0.0822 us | 0.0729 us |  3.61 |    0.03 | 0.4578 |    2937 B |       14.05 |
| Elephant.Uuidv5Utilities | 3.507 us | 0.0105 us | 0.0088 us |  1.30 |    0.00 | 0.2403 |    1533 B |        7.33 |
| GuidPhantom              | 3.904 us | 0.0128 us | 0.0120 us |  1.45 |    0.01 | 0.2441 |    1581 B |        7.56 |
| unique                   | 4.786 us | 0.0248 us | 0.0220 us |  1.77 |    0.01 | 0.2975 |    1910 B |        9.14 |

## Credits
Check out our [list of contributors](https://github.com/MarkCiliaVincenti/DeterministicGuids/blob/master/CONTRIBUTORS.md)!
