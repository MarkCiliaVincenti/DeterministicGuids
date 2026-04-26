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

### .NET 10.0.7 (10.0.7, 10.0.726.21808), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.041 us | 0.0179 us | 0.0158 us |  1.00 |    0.02 |      - |         - |
| UUIDNext                      | 1.105 us | 0.0219 us | 0.0417 us |  1.06 |    0.04 |      - |         - |
| NGuid                         | 1.189 us | 0.0227 us | 0.0465 us |  1.14 |    0.05 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.494 us | 0.0061 us | 0.0054 us |  1.44 |    0.02 | 0.0744 |    1272 B |
| GuidPhantom                   | 1.545 us | 0.0172 us | 0.0366 us |  1.48 |    0.04 | 0.0744 |    1272 B |
| Enbrea.GuidFactory            | 1.653 us | 0.0314 us | 0.0279 us |  1.59 |    0.03 | 0.0763 |    1304 B |
| Elephant.Uuidv5Utilities      | 1.721 us | 0.0191 us | 0.0169 us |  1.65 |    0.03 | 0.0763 |    1304 B |
| unique                        | 1.809 us | 0.0236 us | 0.0221 us |  1.74 |    0.03 | 0.0916 |    1560 B |

### .NET 9.0.15 (9.0.15, 9.0.1526.17522), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.045 us | 0.0192 us | 0.0179 us |  1.00 |    0.02 |      - |         - |
| UUIDNext                      | 1.056 us | 0.0105 us | 0.0088 us |  1.01 |    0.02 |      - |         - |
| NGuid                         | 1.166 us | 0.0159 us | 0.0124 us |  1.12 |    0.02 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.506 us | 0.0087 us | 0.0081 us |  1.44 |    0.02 | 0.0744 |    1272 B |
| GuidPhantom                   | 1.509 us | 0.0103 us | 0.0097 us |  1.44 |    0.03 | 0.0744 |    1272 B |
| Enbrea.GuidFactory            | 1.595 us | 0.0119 us | 0.0111 us |  1.53 |    0.03 | 0.0763 |    1304 B |
| Elephant.Uuidv5Utilities      | 1.703 us | 0.0051 us | 0.0045 us |  1.63 |    0.03 | 0.0763 |    1304 B |
| unique                        | 1.799 us | 0.0094 us | 0.0088 us |  1.72 |    0.03 | 0.0954 |    1600 B |

### .NET 8.0.26 (8.0.26, 8.0.2626.16921), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|-------:|----------:|
| DeterministicGuids            | 1.032 us | 0.0014 us | 0.0012 us |  1.00 |      - |         - |
| UUIDNext                      | 1.050 us | 0.0011 us | 0.0010 us |  1.02 |      - |         - |
| NGuid                         | 1.147 us | 0.0021 us | 0.0018 us |  1.11 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.531 us | 0.0052 us | 0.0046 us |  1.48 | 0.0744 |    1264 B |
| GuidPhantom                   | 1.529 us | 0.0071 us | 0.0063 us |  1.48 | 0.0744 |    1264 B |
| Enbrea.GuidFactory            | 1.615 us | 0.0077 us | 0.0068 us |  1.57 | 0.0763 |    1296 B |
| Elephant.Uuidv5Utilities      | 1.671 us | 0.0084 us | 0.0078 us |  1.62 | 0.0763 |    1296 B |
| unique                        | 1.818 us | 0.0132 us | 0.0117 us |  1.76 | 0.0935 |    1592 B |

### .NET 7.0.20 (7.0.20, 7.0.2024.26716), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|-------:|----------:|
| DeterministicGuids            | 1.064 us | 0.0023 us | 0.0018 us |  1.00 |      - |         - |
| UUIDNext                      | 1.093 us | 0.0009 us | 0.0008 us |  1.03 |      - |         - |
| NGuid                         | 1.176 us | 0.0015 us | 0.0013 us |  1.11 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.589 us | 0.0109 us | 0.0102 us |  1.49 | 0.0744 |    1264 B |
| GuidPhantom                   | 1.584 us | 0.0049 us | 0.0038 us |  1.49 | 0.0744 |    1264 B |
| Elephant.Uuidv5Utilities      | 1.758 us | 0.0115 us | 0.0102 us |  1.65 | 0.0763 |    1296 B |
| unique                        | 1.999 us | 0.0065 us | 0.0058 us |  1.88 | 0.0916 |    1592 B |

### .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.084 us | 0.0141 us | 0.0125 us |  1.00 |    0.02 |      - |         - |
| UUIDNext                      | 1.109 us | 0.0013 us | 0.0013 us |  1.02 |    0.01 |      - |         - |
| NGuid                         | 1.314 us | 0.0032 us | 0.0027 us |  1.21 |    0.01 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.540 us | 0.0037 us | 0.0033 us |  1.42 |    0.02 | 0.0744 |    1264 B |
| GuidPhantom                   | 1.548 us | 0.0065 us | 0.0061 us |  1.43 |    0.02 | 0.0744 |    1264 B |
| Elephant.Uuidv5Utilities      | 1.724 us | 0.0062 us | 0.0055 us |  1.59 |    0.02 | 0.0763 |    1296 B |
| unique                        | 1.942 us | 0.0085 us | 0.0076 us |  1.79 |    0.02 | 0.0916 |    1592 B |

### .NET 5.0.17 (5.0.17, 5.0.1722.21314), X64 RyuJIT x86-64-v3
| Method                   | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated |
|------------------------- |---------:|----------:|----------:|------:|-------:|----------:|
| DeterministicGuids       | 1.087 us | 0.0012 us | 0.0010 us |  1.00 |      - |         - |
| UUIDNext                 | 1.164 us | 0.0011 us | 0.0010 us |  1.07 |      - |         - |
| NGuid                    | 1.730 us | 0.0072 us | 0.0064 us |  1.59 | 0.0820 |    1376 B |
| GuidPhantom              | 1.560 us | 0.0062 us | 0.0055 us |  1.44 | 0.0744 |    1264 B |
| Elephant.Uuidv5Utilities | 1.757 us | 0.0046 us | 0.0041 us |  1.62 | 0.0763 |    1296 B |
| unique                   | 1.928 us | 0.0105 us | 0.0098 us |  1.77 | 0.0916 |    1592 B |

### .NET Core 3.1.32 (3.1.32, 4.700.22.55902), X64 RyuJIT VectorSize=256
| Method                   | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated |
|------------------------- |---------:|----------:|----------:|------:|-------:|----------:|
| DeterministicGuids       | 1.140 us | 0.0014 us | 0.0012 us |  1.00 |      - |         - |
| UUIDNext                 | 1.152 us | 0.0008 us | 0.0007 us |  1.01 |      - |         - |
| NGuid                    | 1.857 us | 0.0091 us | 0.0081 us |  1.63 | 0.0820 |    1376 B |
| GuidPhantom              | 1.682 us | 0.0083 us | 0.0070 us |  1.48 | 0.0744 |    1264 B |
| Elephant.Uuidv5Utilities | 1.887 us | 0.0110 us | 0.0103 us |  1.66 | 0.0763 |    1296 B |
| unique                   | 2.441 us | 0.0080 us | 0.0071 us |  2.14 | 0.0916 |    1592 B |

### .NET Framework 4.8.1 (4.8.9310.0), X64 RyuJIT VectorSize=256
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| DeterministicGuids       | 2.730 us | 0.0054 us | 0.0045 us |  1.00 |    0.00 | 0.0305 |     209 B |        1.00 |
| UUIDNext                 | 2.551 us | 0.0138 us | 0.0123 us |  0.93 |    0.00 | 0.1106 |     698 B |        3.34 |
| NGuid                    | 9.555 us | 0.0803 us | 0.0751 us |  3.50 |    0.03 | 0.4578 |    2937 B |       14.05 |
| GuidPhantom              | 3.887 us | 0.0122 us | 0.0114 us |  1.42 |    0.00 | 0.2441 |    1581 B |        7.56 |
| Elephant.Uuidv5Utilities | 3.645 us | 0.0115 us | 0.0096 us |  1.34 |    0.00 | 0.2403 |    1533 B |        7.33 |
| unique                   | 4.892 us | 0.0118 us | 0.0098 us |  1.79 |    0.00 | 0.2975 |    1910 B |        9.14 |

## Credits
Check out our [list of contributors](https://github.com/MarkCiliaVincenti/DeterministicGuids/blob/master/CONTRIBUTORS.md)!
