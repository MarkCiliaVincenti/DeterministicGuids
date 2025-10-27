# ![DeterministicGuids](https://raw.githubusercontent.com/MarkCiliaVincenti/DeterministicGuids/master/logo32.png)&nbsp;DeterministicGuids
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/MarkCiliaVincenti/DeterministicGuids/dotnet.yml?branch=master&logo=github&style=flat)](https://actions-badge.atrox.dev/MarkCiliaVincenti/DeterministicGuids/goto?ref=master) [![NuGet](https://img.shields.io/nuget/v/DeterministicGuids?label=NuGet&logo=nuget&style=flat)](https://www.nuget.org/packages/DeterministicGuids) [![NuGet](https://img.shields.io/nuget/dt/DeterministicGuids?logo=nuget&style=flat)](https://www.nuget.org/packages/DeterministicGuids) [![Codacy Grade](https://img.shields.io/codacy/grade/d49ddf9069194a939b7f690b6a95199e?style=flat)](https://app.codacy.com/gh/MarkCiliaVincenti/DeterministicGuids/dashboard) [![Codecov](https://img.shields.io/codecov/c/github/MarkCiliaVincenti/DeterministicGuids?label=coverage&logo=codecov&style=flat)](https://app.codecov.io/gh/MarkCiliaVincenti/DeterministicGuids)

A small, allocation-conscious, thread-safe .NET utility for generating **name-based deterministic UUIDs** (a.k.a. GUIDs) using RFC 4122 version 3 (MD5) and version 5 (SHA-1).

You give it:
- a *namespace GUID* (for a logical domain like "Orders", "Users", "Events")
- a *name* (string within that namespace)
- and (optionally) the UUID *version* (3 or 5).  
  If you don't specify it, it defaults to version **5** (SHA-1).

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
  Implements RFC 4122 §4.3 for UUIDv3 (MD5-based) and UUIDv5 (SHA-1-based).

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
  - `net8.0` (and above)

On newer targets we take advantage of newer BCL APIs to reduce allocations to near zero.

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
DeterministicGuids stacks very well against other libraries in terms of speed, whilst being allocation-free. Here are some benchmarks with values taken directly from a run in GitHub Actions running .NET 8.0.

| Method                                                     | Mean     | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------------------------------------- |---------:|----------:|----------:|------:|-------:|----------:|------------:|
| DeterministicGuids                                         | 1.074 us | 0.0009 us | 0.0008 us |  1.00 |      - |         - |          NA |
| Be.Vlaanderen.Basisregisters.Generators.Guid.Deterministic | 1.652 us | 0.0024 us | 0.0021 us |  1.54 | 0.0496 |    1264 B |          NA |
| UUIDNext                                                   | 1.213 us | 0.0012 us | 0.0011 us |  1.13 | 0.0381 |     960 B |          NA |
| NGuid                                                      | 1.204 us | 0.0015 us | 0.0013 us |  1.12 |      - |         - |          NA |
| Elephant.Uuidv5Utilities                                   | 1.839 us | 0.0037 us | 0.0031 us |  1.71 | 0.0515 |    1296 B |          NA |
| Enbrea.GuidFactory                                         | 1.757 us | 0.0031 us | 0.0027 us |  1.64 | 0.0515 |    1296 B |          NA |
| GuidPhantom                                                | 1.666 us | 0.0024 us | 0.0023 us |  1.55 | 0.0496 |    1264 B |          NA |
| unique                                                     | 1.975 us | 0.0035 us | 0.0029 us |  1.84 | 0.0610 |    1592 B |          NA |

## Credits
Check out our [list of contributors](https://github.com/MarkCiliaVincenti/DeterministicGuids/blob/master/CONTRIBUTORS.md)!
