// Copyright (c) All contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;

namespace DeterministicGuids.Tests
{
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
    }
}
