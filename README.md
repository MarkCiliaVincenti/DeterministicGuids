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

  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.019 us | 0.0050 us | 0.0047 us |  1.00 |    0.01 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.497 us | 0.0223 us | 0.0209 us |  1.47 |    0.02 | 0.0744 |    1272 B |
| UUIDNext                      | 1.040 us | 0.0090 us | 0.0075 us |  1.02 |    0.01 |      - |         - |
| NGuid                         | 1.141 us | 0.0028 us | 0.0023 us |  1.12 |    0.01 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.604 us | 0.0129 us | 0.0120 us |  1.57 |    0.01 | 0.0763 |    1304 B |
| Enbrea.GuidFactory            | 1.588 us | 0.0092 us | 0.0081 us |  1.56 |    0.01 | 0.0763 |    1304 B |
| GuidPhantom                   | 1.481 us | 0.0092 us | 0.0077 us |  1.45 |    0.01 | 0.0744 |    1272 B |
| unique                        | 1.716 us | 0.0089 us | 0.0074 us |  1.68 |    0.01 | 0.0916 |    1560 B |

  DefaultJob : .NET 9.0.11 (9.0.11, 9.0.1125.51716), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|-------:|----------:|
| DeterministicGuids            | 1.018 us | 0.0032 us | 0.0026 us |  1.00 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.493 us | 0.0134 us | 0.0125 us |  1.47 | 0.0744 |    1272 B |
| UUIDNext                      | 1.045 us | 0.0087 us | 0.0082 us |  1.03 |      - |         - |
| NGuid                         | 1.146 us | 0.0039 us | 0.0034 us |  1.13 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.625 us | 0.0100 us | 0.0084 us |  1.60 | 0.0763 |    1304 B |
| Enbrea.GuidFactory            | 1.577 us | 0.0075 us | 0.0067 us |  1.55 | 0.0763 |    1304 B |
| GuidPhantom                   | 1.517 us | 0.0067 us | 0.0060 us |  1.49 | 0.0744 |    1272 B |
| unique                        | 1.797 us | 0.0074 us | 0.0066 us |  1.76 | 0.0954 |    1600 B |

  DefaultJob : .NET 8.0.22 (8.0.22, 8.0.2225.52707), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.025 us | 0.0012 us | 0.0011 us |  1.00 |    0.00 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.552 us | 0.0307 us | 0.0354 us |  1.51 |    0.03 | 0.0744 |    1264 B |
| UUIDNext                      | 1.075 us | 0.0100 us | 0.0094 us |  1.05 |    0.01 |      - |         - |
| NGuid                         | 1.157 us | 0.0035 us | 0.0031 us |  1.13 |    0.00 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.653 us | 0.0047 us | 0.0042 us |  1.61 |    0.00 | 0.0763 |    1296 B |
| Enbrea.GuidFactory            | 1.676 us | 0.0333 us | 0.0370 us |  1.63 |    0.04 | 0.0763 |    1296 B |
| GuidPhantom                   | 1.514 us | 0.0054 us | 0.0051 us |  1.48 |    0.01 | 0.0744 |    1264 B |
| unique                        | 1.852 us | 0.0203 us | 0.0190 us |  1.81 |    0.02 | 0.0935 |    1592 B |

  DefaultJob : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.089 us | 0.0208 us | 0.0223 us |  1.00 |    0.03 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.700 us | 0.0278 us | 0.0260 us |  1.56 |    0.04 | 0.0744 |    1264 B |
| UUIDNext                      | 1.197 us | 0.0161 us | 0.0151 us |  1.10 |    0.03 |      - |         - |
| NGuid                         | 1.217 us | 0.0240 us | 0.0359 us |  1.12 |    0.04 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.759 us | 0.0036 us | 0.0030 us |  1.62 |    0.03 | 0.0763 |    1296 B |
| GuidPhantom                   | 1.626 us | 0.0278 us | 0.0320 us |  1.49 |    0.04 | 0.0744 |    1264 B |
| unique                        | 1.932 us | 0.0070 us | 0.0059 us |  1.77 |    0.04 | 0.0916 |    1592 B |

  DefaultJob : .NET 5.0.17 (5.0.17, 5.0.1722.21314), X64 RyuJIT x86-64-v3
| Method                   | Mean     | Error     | StdDev    | Median   | Ratio | RatioSD | Gen0   | Allocated |
|------------------------- |---------:|----------:|----------:|---------:|------:|--------:|-------:|----------:|
| DeterministicGuids       | 1.093 us | 0.0063 us | 0.0157 us | 1.088 us |  1.00 |    0.02 |      - |         - |
| UUIDNext                 | 1.163 us | 0.0158 us | 0.0140 us | 1.160 us |  1.06 |    0.02 |      - |         - |
| NGuid                    | 1.757 us | 0.0209 us | 0.0185 us | 1.750 us |  1.61 |    0.03 | 0.0801 |    1376 B |
| Elephant.Uuidv5Utilities | 1.883 us | 0.0295 us | 0.0276 us | 1.877 us |  1.72 |    0.03 | 0.0763 |    1296 B |
| GuidPhantom              | 1.635 us | 0.0225 us | 0.0211 us | 1.632 us |  1.50 |    0.03 | 0.0744 |    1264 B |
| unique                   | 1.990 us | 0.0280 us | 0.0234 us | 1.999 us |  1.82 |    0.03 | 0.0916 |    1592 B |

  DefaultJob : .NET Core 3.1.32 (3.1.32, 4.700.22.55902), X64 RyuJIT VectorSize=256
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids       | 1.132 us | 0.0122 us | 0.0102 us |  1.00 |    0.01 |      - |         - |
| UUIDNext                 | 1.166 us | 0.0222 us | 0.0186 us |  1.03 |    0.02 |      - |         - |
| NGuid                    | 1.835 us | 0.0212 us | 0.0199 us |  1.62 |    0.02 | 0.0820 |    1376 B |
| Elephant.Uuidv5Utilities | 1.890 us | 0.0103 us | 0.0097 us |  1.67 |    0.02 | 0.0763 |    1296 B |
| GuidPhantom              | 1.696 us | 0.0117 us | 0.0110 us |  1.50 |    0.02 | 0.0744 |    1264 B |
| unique                   | 2.434 us | 0.0152 us | 0.0135 us |  2.15 |    0.02 | 0.0916 |    1592 B |

  DefaultJob : .NET Framework 4.8.1 (4.8.9310.0), X64 RyuJIT VectorSize=256
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| DeterministicGuids       | 2.689 us | 0.0048 us | 0.0043 us |  1.00 |    0.00 | 0.0305 |     209 B |        1.00 |
| UUIDNext                 | 2.500 us | 0.0059 us | 0.0052 us |  0.93 |    0.00 | 0.1106 |     698 B |        3.34 |
| NGuid                    | 9.528 us | 0.0649 us | 0.0575 us |  3.54 |    0.02 | 0.4578 |    2937 B |       14.05 |
| Elephant.Uuidv5Utilities | 3.440 us | 0.0125 us | 0.0110 us |  1.28 |    0.00 | 0.2403 |    1533 B |        7.33 |
| GuidPhantom              | 3.896 us | 0.0210 us | 0.0196 us |  1.45 |    0.01 | 0.2441 |    1581 B |        7.56 |
| unique                   | 4.764 us | 0.0133 us | 0.0118 us |  1.77 |    0.01 | 0.2975 |    1910 B |        9.14 |
## Credits
Check out our [list of contributors](https://github.com/MarkCiliaVincenti/DeterministicGuids/blob/master/CONTRIBUTORS.md)!
