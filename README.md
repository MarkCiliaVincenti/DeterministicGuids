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
| Method                        | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|-------:|----------:|
| DeterministicGuids            | 1.012 us | 0.0033 us | 0.0027 us |  1.00 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.469 us | 0.0069 us | 0.0061 us |  1.45 | 0.0744 |    1272 B |
| UUIDNext                      | 1.028 us | 0.0032 us | 0.0025 us |  1.02 |      - |         - |
| NGuid                         | 1.153 us | 0.0149 us | 0.0139 us |  1.14 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.603 us | 0.0102 us | 0.0091 us |  1.58 | 0.0763 |    1304 B |
| Enbrea.GuidFactory            | 1.596 us | 0.0073 us | 0.0061 us |  1.58 | 0.0763 |    1304 B |
| GuidPhantom                   | 1.491 us | 0.0116 us | 0.0097 us |  1.47 | 0.0744 |    1272 B |
| unique                        | 1.705 us | 0.0073 us | 0.0069 us |  1.68 | 0.0916 |    1560 B |

  DefaultJob : .NET 9.0.11 (9.0.11, 9.0.1125.51716), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.045 us | 0.0149 us | 0.0132 us |  1.00 |    0.02 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.486 us | 0.0080 us | 0.0063 us |  1.42 |    0.02 | 0.0744 |    1272 B |
| UUIDNext                      | 1.053 us | 0.0176 us | 0.0164 us |  1.01 |    0.02 |      - |         - |
| NGuid                         | 1.145 us | 0.0011 us | 0.0010 us |  1.10 |    0.01 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.694 us | 0.0320 us | 0.0404 us |  1.62 |    0.04 | 0.0763 |    1304 B |
| Enbrea.GuidFactory            | 1.627 us | 0.0298 us | 0.0264 us |  1.56 |    0.03 | 0.0763 |    1304 B |
| GuidPhantom                   | 1.521 us | 0.0210 us | 0.0196 us |  1.46 |    0.03 | 0.0744 |    1272 B |
| unique                        | 1.875 us | 0.0190 us | 0.0178 us |  1.79 |    0.03 | 0.0954 |    1600 B |

  DefaultJob : .NET 8.0.22 (8.0.22, 8.0.2225.52707), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.016 us | 0.0015 us | 0.0013 us |  1.00 |    0.00 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.565 us | 0.0308 us | 0.0411 us |  1.54 |    0.04 | 0.0744 |    1264 B |
| UUIDNext                      | 1.141 us | 0.0228 us | 0.0263 us |  1.12 |    0.03 |      - |         - |
| NGuid                         | 1.259 us | 0.0203 us | 0.0190 us |  1.24 |    0.02 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.707 us | 0.0293 us | 0.0260 us |  1.68 |    0.02 | 0.0763 |    1296 B |
| Enbrea.GuidFactory            | 1.614 us | 0.0044 us | 0.0039 us |  1.59 |    0.00 | 0.0763 |    1296 B |
| GuidPhantom                   | 1.565 us | 0.0172 us | 0.0355 us |  1.54 |    0.03 | 0.0744 |    1264 B |
| unique                        | 1.880 us | 0.0339 us | 0.0300 us |  1.85 |    0.03 | 0.0935 |    1592 B |

  DefaultJob : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.064 us | 0.0123 us | 0.0109 us |  1.00 |    0.01 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.599 us | 0.0304 us | 0.0284 us |  1.50 |    0.03 | 0.0744 |    1264 B |
| UUIDNext                      | 1.120 us | 0.0085 us | 0.0071 us |  1.05 |    0.01 |      - |         - |
| NGuid                         | 1.195 us | 0.0115 us | 0.0102 us |  1.12 |    0.01 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.796 us | 0.0278 us | 0.0260 us |  1.69 |    0.03 | 0.0763 |    1296 B |
| GuidPhantom                   | 1.562 us | 0.0232 us | 0.0205 us |  1.47 |    0.02 | 0.0725 |    1264 B |
| unique                        | 1.955 us | 0.0134 us | 0.0112 us |  1.84 |    0.02 | 0.0916 |    1592 B |

  DefaultJob : .NET 5.0.17 (5.0.17, 5.0.1722.21314), X64 RyuJIT x86-64-v3
| Method                   | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated |
|------------------------- |---------:|----------:|----------:|------:|-------:|----------:|
| DeterministicGuids       | 1.063 us | 0.0012 us | 0.0010 us |  1.00 |      - |         - |
| UUIDNext                 | 1.162 us | 0.0011 us | 0.0010 us |  1.09 |      - |         - |
| NGuid                    | 1.688 us | 0.0092 us | 0.0086 us |  1.59 | 0.0820 |    1376 B |
| Elephant.Uuidv5Utilities | 1.752 us | 0.0068 us | 0.0057 us |  1.65 | 0.0763 |    1296 B |
| GuidPhantom              | 1.562 us | 0.0080 us | 0.0071 us |  1.47 | 0.0744 |    1264 B |
| unique                   | 1.932 us | 0.0027 us | 0.0023 us |  1.82 | 0.0935 |    1592 B |

  DefaultJob : .NET Core 3.1.32 (3.1.32, 4.700.22.55902), X64 RyuJIT VectorSize=256
| Method                   | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated |
|------------------------- |---------:|----------:|----------:|------:|-------:|----------:|
| DeterministicGuids       | 1.128 us | 0.0015 us | 0.0013 us |  1.00 |      - |         - |
| UUIDNext                 | 1.151 us | 0.0005 us | 0.0004 us |  1.02 |      - |         - |
| NGuid                    | 1.829 us | 0.0067 us | 0.0059 us |  1.62 | 0.0820 |    1376 B |
| Elephant.Uuidv5Utilities | 1.908 us | 0.0123 us | 0.0115 us |  1.69 | 0.0763 |    1296 B |
| GuidPhantom              | 1.691 us | 0.0103 us | 0.0096 us |  1.50 | 0.0744 |    1264 B |
| unique                   | 2.410 us | 0.0080 us | 0.0067 us |  2.14 | 0.0916 |    1592 B |

  DefaultJob : .NET Framework 4.8.1 (4.8.9310.0), X64 RyuJIT VectorSize=256
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| DeterministicGuids       | 2.692 us | 0.0041 us | 0.0037 us |  1.00 |    0.00 | 0.0305 |     209 B |        1.00 |
| UUIDNext                 | 2.532 us | 0.0096 us | 0.0090 us |  0.94 |    0.00 | 0.1106 |     698 B |        3.34 |
| NGuid                    | 9.660 us | 0.0845 us | 0.0790 us |  3.59 |    0.03 | 0.4578 |    2937 B |       14.05 |
| Elephant.Uuidv5Utilities | 3.469 us | 0.0313 us | 0.0293 us |  1.29 |    0.01 | 0.2403 |    1533 B |        7.33 |
| GuidPhantom              | 4.296 us | 0.0219 us | 0.0205 us |  1.60 |    0.01 | 0.2441 |    1581 B |        7.56 |
| unique                   | 4.708 us | 0.0112 us | 0.0094 us |  1.75 |    0.00 | 0.2975 |    1910 B |        9.14 |
## Credits
Check out our [list of contributors](https://github.com/MarkCiliaVincenti/DeterministicGuids/blob/master/CONTRIBUTORS.md)!
