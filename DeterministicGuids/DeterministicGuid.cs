// Copyright (c) All contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace DeterministicGuids
{
    /// <summary>
    /// Deterministic, namespace+name based UUIDs (RFC 4122 §4.3, v3 MD5 / v5 SHA-1).
    /// </summary>
#if NET8_0_OR_GREATER
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
            /// UUIDv3 (MD5 hashing)
            /// </summary>
            MD5 = 3,

            /// <summary>
            /// UUIDv5 (SHA-1 hashing)
            /// </summary>
            SHA1 = 5
        }

        /// <summary>
        /// Create a deterministic UUID (default: version 5 / SHA-1).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid Create(Guid namespaceId, string name)
            => Create(namespaceId, name, Version.SHA1);

        /// <summary>
        /// Create a deterministic UUID.
        /// </summary>
        /// <param name="namespaceId">Namespace GUID (must not be Guid.Empty).</param>
        /// <param name="name">Name within the namespace (UTF-8 encoded).</param>
        /// <param name="version">Deterministic UUID version to generate (v3 MD5 or v5 SHA-1).</param>
        public static Guid Create(Guid namespaceId, string name, Version version)
        {
            // validate
            if (namespaceId == Guid.Empty)
                throw new ArgumentException("Namespace cannot be an empty GUID.", nameof(namespaceId));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name), "Name cannot be null or empty.");

            // numeric version 3 or 5
            var numericVersion = (int)version;

            // We'll branch per TFM to choose the best implementation we can.
#if NETSTANDARD2_0
            // 1. Get namespace bytes.
            //    netstandard2.0 does not expose Guid.TryWriteBytes,
            //    so ToByteArray() allocates. We copy into a stack buffer after.
            Span<byte> nsBytes = stackalloc byte[16];
            var nsArray = namespaceId.ToByteArray(); // allocates 16 bytes
            nsArray.AsSpan().CopyTo(nsBytes);

            // Swap to RFC 4122 network byte order for hashing
            SwapByteOrder(nsBytes);

            // 2. Encode name as UTF8 (older overload, no span support here)
            int byteCount = Encoding.UTF8.GetByteCount(name);
            byte[] rentedName = ArrayPool<byte>.Shared.Rent(byteCount);
            int bytesWritten = Encoding.UTF8.GetBytes(name, 0, name.Length, rentedName, 0);
            Span<byte> nameBytes = rentedName.AsSpan(0, bytesWritten);

            Guid result;
            try
            {
                // 3. Concatenate nsBytes + nameBytes into one pooled buffer, hash once
                int totalLen = 16 + bytesWritten;
                byte[] rentedCombo = ArrayPool<byte>.Shared.Rent(totalLen);

                try
                {
                    Span<byte> combo = rentedCombo.AsSpan(0, totalLen);

                    nsBytes.CopyTo(combo.Slice(0, 16));
                    nameBytes.CopyTo(combo.Slice(16, bytesWritten));

                    using var hashAlg =
                        version == Version.MD5
                            ? (HashAlgorithm)MD5.Create()
                            : SHA1.Create();

                    byte[] hashArray = hashAlg.ComputeHash(rentedCombo, 0, totalLen); // ~16 or 20 bytes

                    // Copy first 16 bytes of hash into guidBytes
                    Span<byte> guidBytes = stackalloc byte[16];
                    hashArray.AsSpan(0, 16).CopyTo(guidBytes);

                    // Set version nibble (4 high bits of time_hi_and_version)
                    guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | (numericVersion << 4));

                    // Set variant bits (10xxxxxx in clock_seq_hi_and_reserved)
                    guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

                    // Swap back from network order to Guid's internal little-endian layout
                    SwapByteOrder(guidBytes);

                    // netstandard2.0 doesn't have Guid(ReadOnlySpan<byte>) ctor,
                    // so we must allocate an array to build the Guid.
                    byte[] finalBytes = guidBytes.ToArray();
                    result = new Guid(finalBytes);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(rentedCombo);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentedName);
            }

            return result;
#else
            // 1. namespace GUID -> stack buffer
            Span<byte> nsBytes = stackalloc byte[16];
            namespaceId.TryWriteBytes(nsBytes); // no alloc
            SwapByteOrder(nsBytes);             // RFC 4122 requires network byte order before hashing

            // 2. name -> ArrayPool-backed UTF8 bytes (Span-friendly overloads available here)
            int byteCount = Encoding.UTF8.GetByteCount(name);
            byte[] rentedName = ArrayPool<byte>.Shared.Rent(byteCount);
            Span<byte> nameBytes = rentedName.AsSpan(0, byteCount);
            Encoding.UTF8.GetBytes(name, nameBytes);

            Guid result;
            try
            {
                // 3. Compute hash(namespace || name) incrementally (no concat buffer)
                Span<byte> hashBuffer = stackalloc byte[20]; // SHA-1 is 20 bytes; MD5 is 16

                using (var hasher = IncrementalHash.CreateHash(
                           version == Version.MD5 ? HashAlgorithmName.MD5 : HashAlgorithmName.SHA1))
                {
                    hasher.AppendData(nsBytes);
                    hasher.AppendData(nameBytes);

                    if (!hasher.TryGetHashAndReset(hashBuffer, out _))
                        throw new InvalidOperationException("Hash computation failed.");
                }

                // 4. First 16 bytes of hash becomes the basis of the GUID
                Span<byte> guidBytes = stackalloc byte[16];
                hashBuffer[..16].CopyTo(guidBytes);

                // Set version nibble (time_hi_and_version high 4 bits)
                guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | (numericVersion << 4));

                // Set variant bits (clock_seq_hi_and_reserved top bits 10xxxxxx)
                guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

                // Swap from network to Guid's internal ordering
                SwapByteOrder(guidBytes);

                result = new Guid(guidBytes);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentedName);
            }

            return result;
#endif
        }

        /// <summary>
        /// Swap Guid byte layout between little-endian struct layout and RFC 4122 network order.
        /// Swaps:
        ///   time_low (0..3),
        ///   time_mid (4..5),
        ///   time_hi_and_version (6..7).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SwapByteOrder(Span<byte> guidBytes)
        {
            // time_low
            Swap(guidBytes, 0, 3);
            Swap(guidBytes, 1, 2);

            // time_mid
            Swap(guidBytes, 4, 5);

            // time_hi_and_version
            Swap(guidBytes, 6, 7);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(Span<byte> bytes, int a, int b)
        {
            byte tmp = bytes[a];
            bytes[a] = bytes[b];
            bytes[b] = tmp;
        }
    }
}
