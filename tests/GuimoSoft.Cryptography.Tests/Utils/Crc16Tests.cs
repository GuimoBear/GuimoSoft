using FluentAssertions;
using System;
using GuimoSoft.Cryptography.RSA.Utils;
using Xunit;

namespace GuimoSoft.Cryptography.Tests.Utils
{
    public class Crc16Tests
    {
        [Fact]
        public void ComputeFacts()
        {
            ushort expected = 8;

            Crc16.Compute(Array.Empty<byte>(), expected)
                .Should().Be(expected);
        }
    }
}
