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

  DefaultJob : .NET 10.0.2 (10.0.2, 10.0.225.61305), X64 RyuJIT x86-64-v4
| Method                        | Mean     | Error     | StdDev    | Median   | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|---------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.078 us | 0.0114 us | 0.0101 us | 1.073 us |  1.00 |    0.01 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.585 us | 0.0080 us | 0.0071 us | 1.583 us |  1.47 |    0.01 | 0.0496 |    1272 B |
| UUIDNext                      | 1.086 us | 0.0018 us | 0.0017 us | 1.086 us |  1.01 |    0.01 |      - |         - |
| NGuid                         | 1.247 us | 0.0247 us | 0.0505 us | 1.226 us |  1.16 |    0.05 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.805 us | 0.0117 us | 0.0098 us | 1.806 us |  1.68 |    0.02 | 0.0515 |    1304 B |
| Enbrea.GuidFactory            | 1.845 us | 0.0368 us | 0.1002 us | 1.813 us |  1.71 |    0.09 | 0.0515 |    1304 B |
| GuidPhantom                   | 1.628 us | 0.0098 us | 0.0077 us | 1.624 us |  1.51 |    0.02 | 0.0496 |    1272 B |
| unique                        | 1.915 us | 0.0286 us | 0.0267 us | 1.901 us |  1.78 |    0.03 | 0.0610 |    1560 B |

  DefaultJob : .NET 9.0.12 (9.0.12, 9.0.1225.60609), X64 RyuJIT x86-64-v4
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.089 us | 0.0091 us | 0.0076 us |  1.00 |    0.01 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.652 us | 0.0071 us | 0.0066 us |  1.52 |    0.01 | 0.0496 |    1272 B |
| UUIDNext                      | 1.123 us | 0.0202 us | 0.0290 us |  1.03 |    0.03 |      - |         - |
| NGuid                         | 1.202 us | 0.0043 us | 0.0036 us |  1.10 |    0.01 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.817 us | 0.0186 us | 0.0174 us |  1.67 |    0.02 | 0.0496 |    1304 B |
| Enbrea.GuidFactory            | 1.801 us | 0.0298 us | 0.0355 us |  1.65 |    0.03 | 0.0515 |    1304 B |
| GuidPhantom                   | 1.679 us | 0.0252 us | 0.0236 us |  1.54 |    0.02 | 0.0496 |    1272 B |
| unique                        | 2.034 us | 0.0323 us | 0.0303 us |  1.87 |    0.03 | 0.0610 |    1600 B |

  DefaultJob : .NET 8.0.23 (8.0.23, 8.0.2325.60607), X64 RyuJIT x86-64-v4
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.104 us | 0.0220 us | 0.0236 us |  1.00 |    0.03 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.632 us | 0.0157 us | 0.0147 us |  1.48 |    0.03 | 0.0496 |    1264 B |
| UUIDNext                      | 1.139 us | 0.0221 us | 0.0272 us |  1.03 |    0.03 |      - |         - |
| NGuid                         | 1.204 us | 0.0034 us | 0.0029 us |  1.09 |    0.02 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.865 us | 0.0301 us | 0.0267 us |  1.69 |    0.04 | 0.0496 |    1296 B |
| Enbrea.GuidFactory            | 1.809 us | 0.0284 us | 0.0266 us |  1.64 |    0.04 | 0.0515 |    1296 B |
| GuidPhantom                   | 1.701 us | 0.0271 us | 0.0240 us |  1.54 |    0.04 | 0.0496 |    1264 B |
| unique                        | 2.181 us | 0.0432 us | 0.0745 us |  1.98 |    0.08 | 0.0610 |    1592 B |

  DefaultJob : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v4
| Method                        | Mean     | Error     | StdDev    | Median   | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|---------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.139 us | 0.0223 us | 0.0486 us | 1.116 us |  1.00 |    0.06 |      - |         - |
| Be.Vlaanderen...Deterministic | 1.619 us | 0.0174 us | 0.0163 us | 1.620 us |  1.42 |    0.06 | 0.0496 |    1264 B |
| UUIDNext                      | 1.134 us | 0.0016 us | 0.0014 us | 1.134 us |  1.00 |    0.04 |      - |         - |
| NGuid                         | 1.199 us | 0.0023 us | 0.0018 us | 1.199 us |  1.05 |    0.04 |      - |         - |
| Elephant.Uuidv5Utilities      | 1.865 us | 0.0237 us | 0.0403 us | 1.855 us |  1.64 |    0.07 | 0.0515 |    1296 B |
| GuidPhantom                   | 1.683 us | 0.0284 us | 0.0291 us | 1.677 us |  1.48 |    0.06 | 0.0496 |    1264 B |
| unique                        | 2.105 us | 0.0350 us | 0.0328 us | 2.100 us |  1.85 |    0.08 | 0.0610 |    1592 B |

  DefaultJob : .NET 5.0.17 (5.0.17, 5.0.1722.21314), X64 RyuJIT x86-64-v4
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids       | 1.123 us | 0.0153 us | 0.0128 us |  1.00 |    0.02 |      - |         - |
| UUIDNext                 | 1.191 us | 0.0212 us | 0.0198 us |  1.06 |    0.02 |      - |         - |
| NGuid                    | 1.858 us | 0.0231 us | 0.0216 us |  1.65 |    0.03 | 0.0534 |    1376 B |
| Elephant.Uuidv5Utilities | 1.854 us | 0.0248 us | 0.0232 us |  1.65 |    0.03 | 0.0496 |    1296 B |
| GuidPhantom              | 1.632 us | 0.0316 us | 0.0280 us |  1.45 |    0.03 | 0.0496 |    1264 B |
| unique                   | 1.980 us | 0.0158 us | 0.0132 us |  1.76 |    0.02 | 0.0610 |    1592 B |

  DefaultJob : .NET Core 3.1.32 (3.1.32, 4.700.22.55902), X64 RyuJIT VectorSize=256
| Method                   | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated |
|------------------------- |---------:|----------:|----------:|------:|-------:|----------:|
| DeterministicGuids       | 1.128 us | 0.0010 us | 0.0009 us |  1.00 |      - |         - |
| UUIDNext                 | 1.215 us | 0.0015 us | 0.0014 us |  1.08 |      - |         - |
| NGuid                    | 2.003 us | 0.0131 us | 0.0116 us |  1.78 | 0.0534 |    1376 B |
| Elephant.Uuidv5Utilities | 1.983 us | 0.0182 us | 0.0171 us |  1.76 | 0.0496 |    1296 B |
| GuidPhantom              | 1.770 us | 0.0101 us | 0.0095 us |  1.57 | 0.0496 |    1264 B |
| unique                   | 2.565 us | 0.0120 us | 0.0112 us |  2.27 | 0.0610 |    1592 B |

  DefaultJob : .NET Framework 4.8.1 (4.8.9310.0), X64 RyuJIT VectorSize=256
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| DeterministicGuids       | 3.024 us | 0.0129 us | 0.0121 us |  1.00 |    0.01 | 0.0305 |     209 B |        1.00 |
| UUIDNext                 | 3.037 us | 0.0182 us | 0.0170 us |  1.00 |    0.01 | 0.1106 |     698 B |        3.34 |
| NGuid                    | 9.784 us | 0.0493 us | 0.0437 us |  3.24 |    0.02 | 0.4578 |    2937 B |       14.05 |
| Elephant.Uuidv5Utilities | 4.105 us | 0.0184 us | 0.0154 us |  1.36 |    0.01 | 0.2365 |    1533 B |        7.33 |
| GuidPhantom              | 4.694 us | 0.0473 us | 0.0443 us |  1.55 |    0.02 | 0.2441 |    1581 B |        7.56 |
| unique                   | 5.409 us | 0.0653 us | 0.0611 us |  1.79 |    0.02 | 0.2975 |    1910 B |        9.14 |
## Credits
Check out our [list of contributors](https://github.com/MarkCiliaVincenti/DeterministicGuids/blob/master/CONTRIBUTORS.md)!
