// Copyright (c) All contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using NGuid;
using System.Security.Cryptography;
using System.Text;
using UUIDNext;

namespace DeterministicGuids.Tests;

public class DeterministicGuidTests
{
    [Theory]
    // https://www.ietf.org/rfc/rfc4122.txt
    [InlineData("6ba7b810-9dad-11d1-80b4-00c04fd430c8", "www.widgets.com", DeterministicGuid.Version.MD5, "3d813cbb-47fb-32ba-91df-831e1593ac29")]

    // https://docs.python.org/2/library/uuid.html
    [InlineData("6ba7b810-9dad-11d1-80b4-00c04fd430c8", "python.org", DeterministicGuid.Version.MD5, "6fa459ea-ee8a-3ca4-894e-db77e160355e")]
    [InlineData("6ba7b810-9dad-11d1-80b4-00c04fd430c8", "python.org", DeterministicGuid.Version.SHA1, "886313e1-3b8a-5372-9b90-0c9aee199e5d")]

    // https://www.npmjs.com/package/uuid
    [InlineData("6ba7b810-9dad-11d1-80b4-00c04fd430c8", "hello.example.com", DeterministicGuid.Version.SHA1, "fdda765f-fc57-5604-a269-52a7df8164ec")]
    [InlineData("6ba7b811-9dad-11d1-80b4-00c04fd430c8", "http://example.com/hello", DeterministicGuid.Version.SHA1, "3bbcee75-cecc-5b56-8031-b6641c1ed1f1")]
    public void When_generating_a_deterministic_guid(
        string namespaceGuidString,
        string value,
        DeterministicGuid.Version version,
        string resultingGuidString)
    {
        // Arrange
        var namespaceGuid = new Guid(namespaceGuidString);
        var expectedGuid = new Guid(resultingGuidString);

        // Act
        var actual = DeterministicGuid.Create(namespaceGuid, value, version);

        // Assert
        actual.Should().Be(expectedGuid);
    }

    [Fact]
    public void TestNamespaces()
    {
        // Just to make sure they work without exceptions
        var guid1 = DeterministicGuid.Create(DeterministicGuid.Namespaces.Commands, "test");
        var guid2 = DeterministicGuid.Create(DeterministicGuid.Namespaces.Events, "test");
        var guid3 = DeterministicGuid.Create(DeterministicGuid.Namespaces.Dns, "test");
        var guid4 = DeterministicGuid.Create(DeterministicGuid.Namespaces.IsoOid, "test");
        var guid5 = DeterministicGuid.Create(DeterministicGuid.Namespaces.X500Dn, "test");
        var guid6 = DeterministicGuid.Create(DeterministicGuid.Namespaces.Url, "test");

        guid1.Should().Be(new Guid("4aef759d-fda7-5277-b6a4-b921eb736bdf"));
        guid2.Should().Be(new Guid("47817435-6388-5db4-8bc1-8c8d5a10696d"));
        guid3.Should().Be(new Guid("4be0643f-1d98-573b-97cd-ca98a65347dd"));
        guid4.Should().Be(new Guid("b428b5d9-df19-5bb9-a1dc-115e071b836c"));
        guid5.Should().Be(new Guid("63a3ab2b-61b8-5b04-ae2f-70d3875c6e97"));
        guid6.Should().Be(new Guid("da5b8893-d6ca-5c1c-9a9c-91f40a2a3649"));
    }

    [Fact]
    public void TestNamespacesWithLongNames()
    {
        // Just to make sure they work without exceptions
        var guid1 = DeterministicGuid.Create(DeterministicGuid.Namespaces.Commands, "this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible.");
        var guid2 = DeterministicGuid.Create(DeterministicGuid.Namespaces.Events, "this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible.");
        var guid3 = DeterministicGuid.Create(DeterministicGuid.Namespaces.Dns, "this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible.");
        var guid4 = DeterministicGuid.Create(DeterministicGuid.Namespaces.IsoOid, "this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible.");
        var guid5 = DeterministicGuid.Create(DeterministicGuid.Namespaces.X500Dn, "this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible.");
        var guid6 = DeterministicGuid.Create(DeterministicGuid.Namespaces.Url, "this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible. this is a long string because we need to check a different path where stackalloc would not be possible.");

        guid1.Should().Be(new Guid("b44858cb-cf06-5a81-9701-e61aabbb7baa"));
        guid2.Should().Be(new Guid("0c24c5f4-9429-5335-8bb9-7aa8ee739b3b"));
        guid3.Should().Be(new Guid("12733689-6484-55d7-aef6-86025275f50a"));
        guid4.Should().Be(new Guid("9a24d617-c1c1-518e-aff4-ff06e122292d"));
        guid5.Should().Be(new Guid("00b41eb8-0331-5b1c-9217-f301bfb90666"));
        guid6.Should().Be(new Guid("6c3a74ab-4bb1-5350-a809-6f31506699f1"));
    }

    [Fact]
    public void EmptyGuidShouldThrow()
    {
        Action action = () =>
        {
            DeterministicGuid.Create(Guid.Empty, "test");
        };
        action.Should().Throw<ArgumentException>();        
    }

    [Fact]
    public void EmptyNameShouldThrow()
    {
        Action action = () =>
        {
            DeterministicGuid.Create(DeterministicGuid.Namespaces.Dns, "");
        };
        action.Should().Throw<ArgumentNullException>();
    }

#if !NETCOREAPP2_2
    [Fact]
    public void CheckV3CompatibilityWithNGuid()
    {
        var name = "google.com";
        // Act
        var deterministicGuid = DeterministicGuid.Create(DeterministicGuid.Namespaces.Dns, name, DeterministicGuid.Version.MD5);
        var nGuid = GuidHelpers.CreateFromName(GuidHelpers.DnsNamespace, name, 3);
        // Assert
        deterministicGuid.Should().Be(nGuid);
    }
#endif

    [Fact]
    public void CheckV5CompatibilityWithUUIDNext()
    {
        var name = "google.com";
        // Act
        var deterministicGuid = DeterministicGuid.Create(DeterministicGuid.Namespaces.Dns, name);
        var uuidNextGuid = Uuid.NewNameBased(DeterministicGuid.Namespaces.Dns, name);
        // Assert
        deterministicGuid.Should().Be(uuidNextGuid);
    }

#if !NETCOREAPP2_2
    [Fact]
    public void CheckV5CompatibilityWithNGuid()
    {
        var name = "google.com";
        // Act
        var deterministicGuid = DeterministicGuid.Create(DeterministicGuid.Namespaces.Dns, name);
        var nGuid = GuidHelpers.CreateFromName(GuidHelpers.DnsNamespace, name);
        // Assert
        deterministicGuid.Should().Be(nGuid);
    }

    [Fact]
    public void CheckV8CompatibilityWithNGuid()
    {
        var name = "www.example.com";
        // Act
        var deterministicGuid = DeterministicGuid.Create(DeterministicGuid.Namespaces.Dns, name, DeterministicGuid.Version.SHA256);
        var nGuid = GuidHelpers.CreateVersion8FromName(HashAlgorithmName.SHA256,
                GuidHelpers.DnsNamespace, Encoding.ASCII.GetBytes(name));
        // Assert
        deterministicGuid.Should().Be(nGuid);
    }
#endif

    [Fact]
    public void CheckV8CompatibilityWithRFC()
    {
        var name = "www.example.com";
        // Act
        var deterministicGuid = DeterministicGuid.Create(DeterministicGuid.Namespaces.Dns, name, DeterministicGuid.Version.SHA256);
        // Assert
        deterministicGuid.Should().Be(Guid.Parse("5c146b14-3c52-8afd-938a-375d0df1fbf6"));
    }
}
