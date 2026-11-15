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
  - `net8.0`
  - `net11.0` (and above)

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

### .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Median   | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|---------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.019 us | 0.0020 us | 0.0016 us | 1.019 us |  1.00 |    0.00 |      - |         - |
| UUIDNext                      | 1.029 us | 0.0090 us | 0.0080 us | 1.028 us |  1.01 |    0.01 |      - |         - |
| SequentialGuid                | 1.140 us | 0.0027 us | 0.0023 us | 1.140 us |  1.12 |    0.00 |      - |         - |
| NGuid                         | 1.149 us | 0.0121 us | 0.0113 us | 1.142 us |  1.13 |    0.01 |      - |         - |
| Enbrea.GuidFactory            | 1.202 us | 0.0078 us | 0.0073 us | 1.203 us |  1.18 |    0.01 | 0.0305 |     536 B |
| Be.Vlaanderen...Deterministic | 1.479 us | 0.0131 us | 0.0109 us | 1.476 us |  1.45 |    0.01 | 0.0744 |    1272 B |
| GuidPhantom                   | 1.488 us | 0.0129 us | 0.0107 us | 1.485 us |  1.46 |    0.01 | 0.0744 |    1272 B |
| Elephant.Uuidv5Utilities      | 1.703 us | 0.0341 us | 0.1000 us | 1.648 us |  1.67 |    0.10 | 0.0763 |    1304 B |
| unique                        | 1.728 us | 0.0096 us | 0.0080 us | 1.733 us |  1.70 |    0.01 | 0.0916 |    1560 B |

### .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.032 us | 0.0086 us | 0.0081 us |  1.00 |    0.00 |      - |         - |
| UUIDNext                      | 1.044 us | 0.0091 us | 0.0086 us |  1.01 |    0.01 |      - |         - |
| NGuid                         | 1.145 us | 0.0025 us | 0.0020 us |  1.11 |    0.01 |      - |         - |
| SequentialGuid                | 1.150 us | 0.0105 us | 0.0098 us |  1.11 |    0.01 |      - |         - |
| Enbrea.GuidFactory            | 1.204 us | 0.0076 us | 0.0067 us |  1.17 |    0.01 | 0.0305 |     536 B |
| Be.Vlaanderen...Deterministic | 1.502 us | 0.0110 us | 0.0103 us |  1.46 |    0.01 | 0.0744 |    1272 B |
| GuidPhantom                   | 1.514 us | 0.0124 us | 0.0116 us |  1.47 |    0.02 | 0.0744 |    1272 B |
| Elephant.Uuidv5Utilities      | 1.664 us | 0.0237 us | 0.0221 us |  1.61 |    0.02 | 0.0763 |    1304 B |
| unique                        | 1.783 us | 0.0125 us | 0.0117 us |  1.73 |    0.02 | 0.0916 |    1560 B |

### .NET 9.0.23 (9.0.23, 9.0.2326.52216), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.047 us | 0.0105 us | 0.0088 us |  1.00 |    0.01 |      - |         - |
| UUIDNext                      | 1.053 us | 0.0113 us | 0.0105 us |  1.01 |    0.01 |      - |         - |
| NGuid                         | 1.164 us | 0.0113 us | 0.0100 us |  1.11 |    0.01 |      - |         - |
| SequentialGuid                | 1.171 us | 0.0109 us | 0.0102 us |  1.12 |    0.01 |      - |         - |
| Enbrea.GuidFactory            | 1.203 us | 0.0076 us | 0.0071 us |  1.15 |    0.01 | 0.0305 |     536 B |
| GuidPhantom                   | 1.503 us | 0.0059 us | 0.0046 us |  1.44 |    0.01 | 0.0744 |    1272 B |
| Be.Vlaanderen...Deterministic | 1.513 us | 0.0137 us | 0.0128 us |  1.44 |    0.02 | 0.0744 |    1272 B |
| Elephant.Uuidv5Utilities      | 1.652 us | 0.0125 us | 0.0117 us |  1.58 |    0.02 | 0.0763 |    1304 B |
| unique                        | 1.812 us | 0.0141 us | 0.0132 us |  1.73 |    0.02 | 0.0954 |    1600 B |

### .NET 8.0.34 (8.0.34, 8.0.3426.52217), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.033 us | 0.0098 us | 0.0092 us |  1.00 |    0.01 |      - |         - |
| UUIDNext                      | 1.061 us | 0.0098 us | 0.0087 us |  1.03 |    0.01 |      - |         - |
| NGuid                         | 1.156 us | 0.0064 us | 0.0057 us |  1.12 |    0.01 |      - |         - |
| SequentialGuid                | 1.170 us | 0.0074 us | 0.0069 us |  1.13 |    0.01 |      - |         - |
| Enbrea.GuidFactory            | 1.209 us | 0.0051 us | 0.0045 us |  1.17 |    0.01 | 0.0305 |     536 B |
| Be.Vlaanderen...Deterministic | 1.509 us | 0.0126 us | 0.0112 us |  1.46 |    0.02 | 0.0744 |    1264 B |
| GuidPhantom                   | 1.527 us | 0.0119 us | 0.0111 us |  1.48 |    0.02 | 0.0744 |    1264 B |
| Elephant.Uuidv5Utilities      | 1.682 us | 0.0158 us | 0.0148 us |  1.63 |    0.02 | 0.0763 |    1296 B |
| unique                        | 1.846 us | 0.0119 us | 0.0112 us |  1.79 |    0.02 | 0.0935 |    1592 B |

### .NET 7.0.20 (7.0.20, 7.0.2024.26716), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.079 us | 0.0075 us | 0.0070 us |  1.00 |    0.00 |      - |         - |
| UUIDNext                      | 1.097 us | 0.0012 us | 0.0010 us |  1.02 |    0.01 |      - |         - |
| NGuid                         | 1.185 us | 0.0062 us | 0.0058 us |  1.10 |    0.01 |      - |         - |
| SequentialGuid                | 1.533 us | 0.0126 us | 0.0118 us |  1.42 |    0.01 | 0.0458 |     768 B |
| Be.Vlaanderen...Deterministic | 1.598 us | 0.0095 us | 0.0084 us |  1.48 |    0.01 | 0.0744 |    1264 B |
| GuidPhantom                   | 1.608 us | 0.0114 us | 0.0101 us |  1.49 |    0.01 | 0.0744 |    1264 B |
| Elephant.Uuidv5Utilities      | 1.802 us | 0.0184 us | 0.0172 us |  1.67 |    0.02 | 0.0763 |    1296 B |
| unique                        | 1.997 us | 0.0224 us | 0.0210 us |  1.85 |    0.02 | 0.0916 |    1592 B |

### .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
| Method                        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------------ |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids            | 1.070 us | 0.0096 us | 0.0090 us |  1.00 |    0.00 |      - |         - |
| UUIDNext                      | 1.114 us | 0.0074 us | 0.0065 us |  1.04 |    0.01 |      - |         - |
| NGuid                         | 1.200 us | 0.0078 us | 0.0073 us |  1.12 |    0.01 |      - |         - |
| SequentialGuid                | 1.479 us | 0.0134 us | 0.0125 us |  1.38 |    0.02 | 0.0458 |     768 B |
| Be.Vlaanderen...Deterministic | 1.547 us | 0.0077 us | 0.0064 us |  1.45 |    0.01 | 0.0744 |    1264 B |
| GuidPhantom                   | 1.564 us | 0.0162 us | 0.0152 us |  1.46 |    0.02 | 0.0744 |    1264 B |
| Elephant.Uuidv5Utilities      | 1.761 us | 0.0131 us | 0.0123 us |  1.65 |    0.02 | 0.0763 |    1296 B |
| unique                        | 1.985 us | 0.0263 us | 0.0246 us |  1.86 |    0.03 | 0.0916 |    1592 B |

### .NET 5.0.17 (5.0.17, 5.0.1722.21314), X64 RyuJIT x86-64-v3
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids       | 1.069 us | 0.0020 us | 0.0015 us |  1.00 |    0.00 |      - |         - |
| UUIDNext                 | 1.139 us | 0.0016 us | 0.0013 us |  1.07 |    0.00 |      - |         - |
| SequentialGuid           | 1.484 us | 0.0096 us | 0.0080 us |  1.39 |    0.01 | 0.0458 |     768 B |
| GuidPhantom              | 1.584 us | 0.0145 us | 0.0136 us |  1.48 |    0.01 | 0.0744 |    1264 B |
| Elephant.Uuidv5Utilities | 1.801 us | 0.0188 us | 0.0175 us |  1.68 |    0.02 | 0.0763 |    1296 B |
| NGuid                    | 1.819 us | 0.0143 us | 0.0133 us |  1.70 |    0.01 | 0.0820 |    1376 B |
| unique                   | 2.022 us | 0.0161 us | 0.0134 us |  1.89 |    0.01 | 0.0916 |    1592 B |

### .NET Core 3.1.32 (3.1.32, 4.700.22.55902), X64 RyuJIT VectorSize=256
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|
| DeterministicGuids       | 1.137 us | 0.0067 us | 0.0059 us |  1.00 |    0.00 |      - |         - |
| UUIDNext                 | 1.170 us | 0.0102 us | 0.0095 us |  1.03 |    0.01 |      - |         - |
| SequentialGuid           | 1.552 us | 0.0188 us | 0.0176 us |  1.37 |    0.02 | 0.0458 |     768 B |
| GuidPhantom              | 1.696 us | 0.0187 us | 0.0175 us |  1.49 |    0.02 | 0.0744 |    1264 B |
| Elephant.Uuidv5Utilities | 1.930 us | 0.0151 us | 0.0134 us |  1.70 |    0.01 | 0.0763 |    1296 B |
| NGuid                    | 1.884 us | 0.0168 us | 0.0149 us |  1.66 |    0.02 | 0.0801 |    1376 B |
| unique                   | 2.482 us | 0.0165 us | 0.0146 us |  2.18 |    0.02 | 0.0916 |    1592 B |

### .NET Framework 4.8.1 (4.8.9363.0), X64 RyuJIT VectorSize=256
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| DeterministicGuids       | 2.816 us | 0.0134 us | 0.0112 us |  1.00 |    0.00 | 0.0305 | 0.0038 |     209 B |        1.00 |
| UUIDNext                 | 2.661 us | 0.0193 us | 0.0181 us |  0.94 |    0.01 | 0.1106 | 0.0038 |     698 B |        3.34 |
| Elephant.Uuidv5Utilities | 3.661 us | 0.0188 us | 0.0166 us |  1.30 |    0.01 | 0.2403 | 0.0038 |    1533 B |        7.33 |
| SequentialGuid           | 3.426 us | 0.0295 us | 0.0261 us |  1.22 |    0.01 | 0.2480 | 0.0038 |    1573 B |        7.53 |
| GuidPhantom              | 4.101 us | 0.0249 us | 0.0221 us |  1.46 |    0.01 | 0.2441 | 0.0076 |    1581 B |        7.56 |
| unique                   | 5.008 us | 0.0378 us | 0.0335 us |  1.78 |    0.01 | 0.2975 | 0.0076 |    1910 B |        9.14 |
| NGuid                    | 9.998 us | 0.0877 us | 0.0821 us |  3.55 |    0.03 | 0.4578 | 0.0153 |    2937 B |       14.05 |

## Credits
Check out our [list of contributors](https://github.com/MarkCiliaVincenti/DeterministicGuids/blob/master/CONTRIBUTORS.md)!
