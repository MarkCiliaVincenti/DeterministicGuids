// Copyright (c) All contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

#if NETSTANDARD2_0
using System.Buffers;
#endif
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace DeterministicGuids
{
    /// <summary>
    /// Deterministic, namespace+name based UUIDs (RFC 4122 §4.3, v3 MD5 / v5 SHA-1).
    /// </summary>
#if NET5_0_OR_GREATER
    // Micro-opt: skip zeroing stackalloc locals (safe: we always fill before read)
    [SkipLocalsInit]
#endif
    public static class DeterministicGuid
    {
        /// <summary>
        /// Well-known and custom namespaces.
        /// Backed by direct Guid(value parts...) to avoid any runtime string parsing.
        /// </summary>
        public static class Namespaces
        {
            /// <summary>
            /// Represents a deterministic GUID for command operations (b8bfc711-ed0b-4151-a4fd-26a749825f7b).
            /// </summary>
            /// <remarks>This GUID is compatible with the
            /// Be.Vlaanderen.Basisregisters.Generators.Guid.Deterministic library and is used to uniquely identify
            /// command-related operations.</remarks>
            public static readonly Guid Commands = new(
                0xb8bfc711,
                0xed0b,
                0x4151,
                0xa4, 0xfd, 0x26, 0xa7, 0x49, 0x82, 0x5f, 0x7b
            );

            /// <summary>
            /// Represents a unique identifier for events (115a74c3-19dd-4753-b31e-f366eb3e2005).
            /// </summary>
            /// <remarks>This GUID is compatible with the
            /// Be.Vlaanderen.Basisregisters.Generators.Guid.Deterministic library and is used to uniquely identify
            /// event-related operations.</remarks>
            public static readonly Guid Events = new(
                0x115a74c3,
                0x19dd,
                0x4753,
                0xb3, 0x1e, 0xf3, 0x66, 0xeb, 0x3e, 0x20, 0x05
            );

            // RFC 4122 Appendix C

            /// <summary>
            /// Represents the GUID for the DNS namespace as defined in RFC 4122 Appendix C (6ba7b810-9dad-11d1-80b4-00c04fd430c8).
            /// </summary>
            /// <remarks>This GUID is used as a namespace identifier for DNS names when generating
            /// name-based UUIDs (version 3 or 5). It is a constant value and should be used in conjunction with a
            /// name-based UUID generation method to create a UUID that is unique to a specific DNS name.</remarks>
            public static readonly Guid Dns = new(
                0x6ba7b810,
                0x9dad,
                0x11d1,
                0x80, 0xb4, 0x00, 0xc0, 0x4f, 0xd4, 0x30, 0xc8
            );

            /// <summary>
            /// Represents the namespace identifier for URLs as defined in RFC 4122 Appendix C (6ba7b811-9dad-11d1-80b4-00c04fd430c8).
            /// </summary>
            /// <remarks>This GUID is used as a namespace identifier for URLs when generating
            /// name-based UUIDs (version 3 or 5). It is a constant value and should be used in conjunction with a
            /// name-based UUID generation method to create a UUID that is unique to a specific URL.</remarks>
            public static readonly Guid Url = new(
                0x6ba7b811,
                0x9dad,
                0x11d1,
                0x80, 0xb4, 0x00, 0xc0, 0x4f, 0xd4, 0x30, 0xc8
            );

            /// <summary>
            /// Represents the ISO OID namespace identifier as defined in RFC 4122 Appendix C (6ba7b812-9dad-11d1-80b4-00c04fd430c8).
            /// </summary>
            /// <remarks>This <see cref="Guid"/> is used as a namespace identifier for UUIDs generated
            /// according to the ISO OID standard. It is a constant value and should be used when creating UUIDs that
            /// need to be unique within the ISO OID namespace.</remarks>
            public static readonly Guid IsoOid = new(
                0x6ba7b812,
                0x9dad,
                0x11d1,
                0x80, 0xb4, 0x00, 0xc0, 0x4f, 0xd4, 0x30, 0xc8
            );

            /// <summary>
            /// Represents the GUID for the X.500 Distinguished Name (DN) namespace as defined in RFC 4122 Appendix C (6ba7b814-9dad-11d1-80b4-00c04fd430c8).
            /// </summary>
            /// <remarks>This GUID is used as a namespace identifier for UUIDs generated from X.500
            /// Distinguished Names.</remarks>
            public static readonly Guid X500Dn = new(
                0x6ba7b814,
                0x9dad,
                0x11d1,
                0x80, 0xb4, 0x00, 0xc0, 0x4f, 0xd4, 0x30, 0xc8
            );
        }

        /// <summary>
        /// Supported deterministic UUID versions.
        /// </summary>
        public enum Version
        {
            /// <summary>
            /// UUID v3 (MD5 hashing)
            /// </summary>
            MD5 = 3,

            /// <summary>
            /// UUID v5 (SHA-1 hashing)
            /// </summary>
            SHA1 = 5,

            /// <summary>
            /// UUID v8 (SHA-256 hashing)
            /// </summary>
            /// <remarks>UUID v8 is a custom format reserved for vendor-specific use cases, allowing developers to create UUIDs with unique characteristics. This could lead to compatibility issues.</remarks>
            SHA256 = 8
        }

        /// <summary>
        /// Create a deterministic UUID (default: v5 SHA-1).
        /// </summary>
#if NET5_0_OR_GREATER
        [SkipLocalsInit]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid Create(Guid namespaceId, string name)
            => Create(namespaceId, name, Version.SHA1);

        /// <summary>
        /// Create a deterministic UUID.
        /// </summary>
        /// <param name="namespaceId">Namespace GUID (must not be Guid.Empty).</param>
        /// <param name="name">Name within the namespace (UTF-8 encoded).</param>
        /// <param name="version">Deterministic UUID version to generate (v3 MD5, v5 SHA-1 or v8 SHA-256).</param>
#if NET5_0_OR_GREATER
        [SkipLocalsInit]
#endif
        public static Guid Create(Guid namespaceId, string name, Version version)
        {
            if (namespaceId == Guid.Empty)
            {
                throw new ArgumentException("Namespace cannot be an empty GUID.", nameof(namespaceId));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Name cannot be null or empty.");
            }

            int numericVersion = (int)version;

#if NET8_0_OR_GREATER
            // Fastest path:
            // - We can write namespaceId as big-endian directly.
            // - We can build [namespace||name] in one span.
            // - We hash into stackalloc using thread-local HashAlgorithm.TryComputeHash (no per-call alloc).
            // - We set version + variant.
            // - We construct Guid from big-endian bytes using Guid(bigEndian: true).

            // 1. Compute total bytes we will hash: 16-byte namespace + UTF8(name)
            int nameLen = Encoding.UTF8.GetByteCount(name);
            int totalLen = 16 + nameLen;

            Span<byte> concat = totalLen <= 512
                ? stackalloc byte[totalLen]
                : new byte[totalLen];

            // namespace -> big-endian directly into concat[0..16)
            namespaceId.TryWriteBytes(concat[..16], bigEndian: true, out _);

            // UTF8(name) -> concat[16..]
            Encoding.UTF8.GetBytes(name, concat[16..]);

            // 2. Hash namespace||name using per-thread hasher into a stack buffer
            var hasher8 = version switch
            {
                Version.SHA1 => Sha1Tls.Value!,
                Version.SHA256 => Sha256Tls.Value!,
                Version.MD5 => Md5Tls.Value!,
                _ => throw new ArgumentOutOfRangeException(nameof(version))
            };

            int hashLen = hasher8.HashSize / 8; // 16, 20, or 32
            Span<byte> hashBuf = hashLen <= 64 ? stackalloc byte[hashLen] : new byte[hashLen];

            hasher8.TryComputeHash(concat, hashBuf, out _);

            // 3. Take first 16 bytes from hash as the UUID body (big-endian)
            Span<byte> uuidBe = stackalloc byte[16];
            hashBuf[..16].CopyTo(uuidBe);

            // Set version nibble (time_hi_and_version high 4 bits)
            uuidBe[6] = (byte)((uuidBe[6] & 0x0F) | (numericVersion << 4));

            // Set variant bits (10xxxxxx in clock_seq_hi_and_reserved)
            uuidBe[8] = (byte)((uuidBe[8] & 0x3F) | 0x80);

            // 4. .NET 8 Guid ctor can accept big-endian bytes directly
            return new Guid(uuidBe, bigEndian: true);

#elif NET5_0_OR_GREATER || NETSTANDARD2_1
            // Fast path for .NET 5 / .NET 6 / .NET 7 / .NET Standard 2.1:
            // Differences vs .NET 8:
            //   - We cannot write Guid as big-endian directly, so we:
            //       * Write namespaceId to nsLe (internal layout),
            //       * Swap to big-endian in-place.
            //   - We cannot construct Guid from big-endian bytes, so after hashing
            //     we convert big-endian UUID bytes back to Guid layout.
            //   - But: we DO have Guid.TryWriteBytes, Guid(ReadOnlySpan<byte>),
            //     Encoding.GetBytes(Span), HashAlgorithm.TryComputeHash.
            //
            // Result: still zero allocations for normal-size names.

            // 1. namespace in internal layout
            Span<byte> nsLe = stackalloc byte[16];
            namespaceId.TryWriteBytes(nsLe);

            // 2. swap those 16 bytes to RFC4122 big-endian order for hashing
            SwapToBigEndianInPlace(nsLe); // nsLe is now nsBe

            // 3. build [namespace_be || UTF8(name)] in one contiguous span
            int nameLen = Encoding.UTF8.GetByteCount(name);
            int totalLen = 16 + nameLen;

            Span<byte> concat = totalLen <= 512
                ? stackalloc byte[totalLen]
                : new byte[totalLen];

            nsLe.CopyTo(concat[..16]);
            Encoding.UTF8.GetBytes(name.AsSpan(), concat[16..]);

            // 4. hash into stackalloc via thread-local HashAlgorithm
            var hasher6 = version switch
            {
                Version.SHA1 => Sha1Tls.Value!,
                Version.SHA256 => Sha256Tls.Value!,
                Version.MD5 => Md5Tls.Value!,
                _ => throw new ArgumentOutOfRangeException(nameof(version))
            };

            int hashLen = hasher6.HashSize / 8; // 16, 20, or 32
            Span<byte> hashBuf = hashLen <= 64 ? stackalloc byte[hashLen] : new byte[hashLen];

            hasher6.TryComputeHash(concat, hashBuf, out _);

            // 5. Copy first 16 hash bytes into 'be' buffer (big-endian UUID body)
            Span<byte> be = stackalloc byte[16];
            hashBuf[..16].CopyTo(be);

            // set version nibble & variant bits in big-endian view
            be[6] = (byte)((be[6] & 0x0F) | (numericVersion << 4));
            be[8] = (byte)((be[8] & 0x3F) | 0x80);

            // 6. Convert big-endian UUID bytes into Guid's internal layout
            Span<byte> le = stackalloc byte[16];
            ConvertUuidBigEndianToGuidLayout(be, le);

            // 7. net6+/netstandard2.1 both have Guid(ReadOnlySpan<byte>) ctor
            return new Guid(le);

#else
            // .NET Standard 2.0:
            // - No Guid.TryWriteBytes
            // - No Guid(ReadOnlySpan<byte>) ctor
            // - No HashAlgorithm.TryComputeHash
            // Use TransformBlock/TransformFinalBlock to hash [nsBE || nameUTF8]
            // Keep ArrayPool for large-ish buffers; Guid(byte[]) requires a real array.

            var hasher20 = version switch
            {
                Version.SHA1 => Sha1Tls.Value!,
                Version.SHA256 => Sha256Tls.Value!,
                Version.MD5 => Md5Tls.Value!,
                _ => throw new ArgumentOutOfRangeException(nameof(version))
            };

            // Encode name (array overload only on ns2.0)
            int nameLen = Encoding.UTF8.GetByteCount(name);
            byte[] rentedName = ArrayPool<byte>.Shared.Rent(nameLen);
            int written = Encoding.UTF8.GetBytes(name, 0, name.Length, rentedName, 0);

            // Namespace: unavoidable 16B alloc to get internal layout
            byte[] nsLeArr = namespaceId.ToByteArray();

            // Rent a 16B buffer for namespace in big-endian and swap via helper
            byte[] rentedNsBe = ArrayPool<byte>.Shared.Rent(16);
            Guid result;
            try
            {
                // Copy internal layout -> rentedNsBe, then swap to BE
                Buffer.BlockCopy(nsLeArr, 0, rentedNsBe, 0, 16);
                SwapToBigEndianInPlace(rentedNsBe); // uses your Span-based helper

                // Hash(namespaceBE || nameUTF8)
                hasher20.Initialize();
                hasher20.TransformBlock(rentedNsBe, 0, 16, null, 0);
                hasher20.TransformFinalBlock(rentedName, 0, written);

                byte[] hashArr = hasher20.Hash ?? throw new InvalidOperationException("Hash computation failed.");

                // Leftmost 16 bytes -> BE UUID body
                byte[] beArr = ArrayPool<byte>.Shared.Rent(16);
                try
                {
                    Buffer.BlockCopy(hashArr, 0, beArr, 0, 16);

                    // Set version nibble + variant bits in BE view
                    beArr[6] = (byte)((beArr[6] & 0x0F) | (((int)version) << 4));
                    beArr[8] = (byte)((beArr[8] & 0x3F) | 0x80);

                    // Convert BE -> Guid layout using your helper
                    byte[] leArr = ArrayPool<byte>.Shared.Rent(16);
                    try
                    {
                        // helper is Span-based, so wrap arrays as spans
                        ConvertUuidBigEndianToGuidLayout(beArr, leArr);
                        // Guid(byte[]) copies the contents; safe to return array after
                        result = new Guid(leArr);
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(leArr);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(beArr);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentedNsBe);
                ArrayPool<byte>.Shared.Return(rentedName);
            }

            return result;
#endif
        }

        /// <summary>
        /// Swap a GUID byte span from .NET's internal layout (first three fields little-endian)
        /// into RFC 4122 network byte order (big-endian in those fields). Operates in-place.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SwapToBigEndianInPlace(Span<byte> g)
        {
            // time_low (bytes 0..3)
            (g[0], g[3]) = (g[3], g[0]);
            (g[1], g[2]) = (g[2], g[1]);

            // time_mid (bytes 4..5)
            (g[4], g[5]) = (g[5], g[4]);

            // time_hi_and_version (bytes 6..7)
            (g[6], g[7]) = (g[7], g[6]);

            // bytes 8..15 are already in the correct order for network form
        }

        /// <summary>
        /// Convert UUID bytes expressed in big-endian RFC 4122 order into the byte layout
        /// that <see cref="Guid"/> expects in memory. Writes the converted bytes into <paramref name="guidLayout"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ConvertUuidBigEndianToGuidLayout(ReadOnlySpan<byte> uuidBigEndian, Span<byte> guidLayout)
        {
            // uuidBigEndian: [0..3]=time_low (big-endian),
            //                [4..5]=time_mid (big-endian),
            //                [6..7]=time_hi_and_version (big-endian),
            //                [8..15]=the rest.
            //
            // Guid internal layout wants those first fields little-endian.

            // time_low
            guidLayout[0] = uuidBigEndian[3];
            guidLayout[1] = uuidBigEndian[2];
            guidLayout[2] = uuidBigEndian[1];
            guidLayout[3] = uuidBigEndian[0];

            // time_mid
            guidLayout[4] = uuidBigEndian[5];
            guidLayout[5] = uuidBigEndian[4];

            // time_hi_and_version
            guidLayout[6] = uuidBigEndian[7];
            guidLayout[7] = uuidBigEndian[6];

            // rest is already in correct order
            uuidBigEndian.Slice(8, 8).CopyTo(guidLayout.Slice(8, 8));
        }

        // Thread-local hashers:
        // We keep one MD5, one SHA-1 and one SHA-256 per thread so we don't allocate HashAlgorithm
        // objects per call.
        // NOTE: HashAlgorithm instances are not thread-safe, so ThreadLocal is important.
        private static readonly ThreadLocal<HashAlgorithm> Sha1Tls = new(SHA1.Create);
        private static readonly ThreadLocal<HashAlgorithm> Md5Tls = new(MD5.Create);
        private static readonly ThreadLocal<HashAlgorithm> Sha256Tls = new(SHA256.Create);
    }
}
