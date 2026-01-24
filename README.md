# ![DeterministicGuids](https://raw.githubusercontent.com/MarkCiliaVincenti/DeterministicGuids/master/logo32.png)&nbsp;DeterministicGuids
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/MarkCiliaVincenti/DeterministicGuids/dotnet.yml?branch=master&logo=github&style=flat)](https://actions-badge.atrox.dev/MarkCiliaVincenti/DeterministicGuids/goto?ref=master) [![NuGet](https://img.shields.io/nuget/v/DeterministicGuids?label=NuGet&logo=nuget&style=flat)](https://www.nuget.org/packages/DeterministicGuids) [![NuGet](https://img.shields.io/nuget/dt/DeterministicGuids?logo=nuget&style=flat)](https://www.nuget.org/packages/DeterministicGuids) [![Codacy Grade](https://img.shields.io/codacy/grade/d49ddf9069194a939b7f690b6a95199e?style=flat)](https://app.codacy.com/gh/MarkCiliaVincenti/DeterministicGuids/dashboard) [![Codecov](https://img.shields.io/codecov/c/github/MarkCiliaVincenti/DeterministicGuids?label=coverage&logo=codecov&style=flat)](https://app.codecov.io/gh/MarkCiliaVincenti/DeterministicGuids)

A small, allocation-conscious, thread-safe .NET utility for generating **name-based deterministic UUIDs** (a.k.a. GUIDs) using RFC 9562 v3 (MD5), v5 (SHA-1) and v8 (SHA-256).

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
  Implements RFC 9562 §5.3 for UUIDv3 (MD5-based), RFC 9562 §5.5 UUIDv5 (SHA-1-based) and RFC 9562 §5.8 for UUIDv8 (SHA-256-based).

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

### .NET 10.0.2 (10.0.2, 10.0.225.61305), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.014 us | 0.0056 us | 0.0049 us |  1.00 |    0.01 |      - |         - |
| UUIDNext                      | 1.034 us | 0.0010 us | 0.0009 us |  1.02 |    0.00 |      - |         - |
| NGuid                         | 1.150 us | 0.0013 us | 0.0012 us |  1.13 |    0.01 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.487 us | 0.0120 us | 0.0112 us |  1.47 |    0.01 | 0.0744 |    1272 B |
| GuidPhantom                   | 1.482 us | 0.0096 us | 0.0090 us |  1.46 |    0.01 | 0.0744 |    1272 B |
| Enbrea.GuidFactory            | 1.630 us | 0.0131 us | 0.0122 us |  1.61 |    0.01 | 0.0763 |    1304 B |
| Elephant.Uuidv5Utilities      | 1.683 us | 0.0329 us | 0.0416 us |  1.66 |    0.04 | 0.0763 |    1304 B |
| unique                        | 1.726 us | 0.0129 us | 0.0108 us |  1.70 |    0.01 | 0.0916 |    1560 B |

### .NET 9.0.12 (9.0.12, 9.0.1225.60609), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.011 us | 0.0018 us | 0.0016 us |  1.00 |    0.00 |      - |         - |
| UUIDNext                      | 1.041 us | 0.0009 us | 0.0008 us |  1.03 |    0.00 |      - |         - |
| NGuid                         | 1.147 us | 0.0027 us | 0.0021 us |  1.13 |    0.00 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.595 us | 0.0339 us | 0.0999 us |  1.58 |    0.10 | 0.0744 |    1272 B |
| GuidPhantom                   | 1.512 us | 0.0129 us | 0.0114 us |  1.50 |    0.01 | 0.0744 |    1272 B |
| Enbrea.GuidFactory            | 1.598 us | 0.0131 us | 0.0123 us |  1.58 |    0.01 | 0.0763 |    1304 B |
| Elephant.Uuidv5Utilities      | 1.670 us | 0.0098 us | 0.0087 us |  1.65 |    0.01 | 0.0763 |    1304 B |
| unique                        | 1.845 us | 0.0175 us | 0.0155 us |  1.82 |    0.02 | 0.0954 |    1600 B |

### .NET 8.0.23 (8.0.23, 8.0.2325.60607), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.030 us | 0.0027 us | 0.0023 us |  1.00 |    0.00 |      - |         - |
| UUIDNext                      | 1.069 us | 0.0197 us | 0.0175 us |  1.04 |    0.02 |      - |         - |
| NGuid                         | 1.172 us | 0.0094 us | 0.0088 us |  1.14 |    0.01 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.556 us | 0.0301 us | 0.0282 us |  1.51 |    0.03 | 0.0744 |    1264 B |
| GuidPhantom                   | 1.531 us | 0.0185 us | 0.0164 us |  1.49 |    0.02 | 0.0744 |    1264 B |
| Enbrea.GuidFactory            | 1.639 us | 0.0185 us | 0.0154 us |  1.59 |    0.01 | 0.0763 |    1296 B |
| Elephant.Uuidv5Utilities      | 1.675 us | 0.0104 us | 0.0087 us |  1.63 |    0.01 | 0.0763 |    1296 B |
| unique                        | 1.879 us | 0.0357 us | 0.0351 us |  1.82 |    0.03 | 0.0935 |    1592 B |

### .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.067 us | 0.0058 us | 0.0049 us |  1.00 |    0.01 |      - |         - |
| UUIDNext                      | 1.110 us | 0.0030 us | 0.0026 us |  1.04 |    0.01 |      - |         - |
| NGuid                         | 1.331 us | 0.0119 us | 0.0111 us |  1.25 |    0.01 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.561 us | 0.0146 us | 0.0137 us |  1.46 |    0.01 | 0.0744 |    1264 B |
| GuidPhantom                   | 1.611 us | 0.0316 us | 0.0324 us |  1.51 |    0.03 | 0.0744 |    1264 B |
| Elephant.Uuidv5Utilities      | 1.755 us | 0.0209 us | 0.0186 us |  1.64 |    0.02 | 0.0763 |    1296 B |
| unique                        | 1.993 us | 0.0384 us | 0.0340 us |  1.87 |    0.03 | 0.0916 |    1592 B |

### .NET 5.0.17 (5.0.17, 5.0.1722.21314), X64 RyuJIT x86-64-v3
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids       | 1.080 us | 0.0208 us | 0.0185 us |  1.00 |    0.02 |      - |         - |
| UUIDNext                 | 1.152 us | 0.0216 us | 0.0212 us |  1.07 |    0.03 |      - |         - |
| NGuid                    | 1.738 us | 0.0184 us | 0.0172 us |  1.61 |    0.03 | 0.0820 |    1376 B |
| GuidPhantom              | 1.605 us | 0.0285 us | 0.0253 us |  1.49 |    0.03 | 0.0744 |    1264 B |
| Elephant.Uuidv5Utilities | 1.862 us | 0.0371 us | 0.0347 us |  1.73 |    0.04 | 0.0763 |    1296 B |
| unique                   | 2.164 us | 0.0429 us | 0.0680 us |  2.01 |    0.07 | 0.0916 |    1592 B |

### .NET Core 3.1.32 (3.1.32, 4.700.22.55902), X64 RyuJIT VectorSize=256
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids       | 1.133 us | 0.0145 us | 0.0128 us |  1.00 |    0.02 |      - |         - |
| UUIDNext                 | 1.158 us | 0.0026 us | 0.0021 us |  1.02 |    0.01 |      - |         - |
| NGuid                    | 1.867 us | 0.0168 us | 0.0158 us |  1.65 |    0.02 | 0.0801 |    1376 B |
| GuidPhantom              | 1.766 us | 0.0257 us | 0.0489 us |  1.56 |    0.05 | 0.0744 |    1264 B |
| Elephant.Uuidv5Utilities | 1.948 us | 0.0294 us | 0.0275 us |  1.72 |    0.03 | 0.0763 |    1296 B |
| unique                   | 2.481 us | 0.0388 us | 0.0344 us |  2.19 |    0.04 | 0.0916 |    1592 B |

### .NET Framework 4.8.1 (4.8.9310.0), X64 RyuJIT VectorSize=256
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| DeterministicGuids       | 2.792 us | 0.0503 us | 0.0470 us |  1.00 |    0.02 | 0.0305 |     209 B |        1.00 |
| UUIDNext                 | 2.619 us | 0.0334 us | 0.0313 us |  0.94 |    0.02 | 0.1106 |     698 B |        3.34 |
| NGuid                    | 9.806 us | 0.1383 us | 0.1294 us |  3.51 |    0.07 | 0.4578 |    2937 B |       14.05 |
| GuidPhantom              | 4.039 us | 0.0704 us | 0.0810 us |  1.45 |    0.04 | 0.2441 |    1581 B |        7.56 |
| Elephant.Uuidv5Utilities | 3.558 us | 0.0646 us | 0.0604 us |  1.27 |    0.03 | 0.2403 |    1533 B |        7.33 |
| unique                   | 5.210 us | 0.0188 us | 0.0167 us |  1.87 |    0.03 | 0.2975 |    1910 B |        9.14 |

## Credits
Check out our [list of contributors](https://github.com/MarkCiliaVincenti/DeterministicGuids/blob/master/CONTRIBUTORS.md)!
