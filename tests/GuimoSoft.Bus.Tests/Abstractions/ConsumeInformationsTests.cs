using FluentAssertions;
using System;
using GuimoSoft.Bus.Abstractions;
using Xunit;

namespace GuimoSoft.Bus.Tests.Abstractions
{
    public class ConsumeInformationsTests
    {
        [Theory]
        [InlineData(null, "value")]
        [InlineData("", "value")]
        [InlineData(" ", "value")]
        [InlineData("key", null)]
        [InlineData("key", "")]
        [InlineData("key", " ")]
        public void AddHeadersWithInvalidDataShouldThrowArgumentException(string key, string value)
        {
            var sut = new ConsumeInformations(BusName.Kafka, ServerName.Default, "");

            Assert.Throws<ArgumentException>(() => sut.AddHeader(key, value));
        }

        [Fact]
        public void AddHeaderFacts()
        {
            var sut = new ConsumeInformations(BusName.Kafka, ServerName.Default, "");

            sut.Headers
                .Should().BeEmpty();

            sut.AddHeader("key", "value");

            sut.Headers
                .Should().NotBeEmpty();
        }
    }
}
