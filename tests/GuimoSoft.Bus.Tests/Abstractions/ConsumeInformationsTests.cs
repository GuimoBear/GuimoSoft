using FluentAssertions;
using System;
using GuimoSoft.Bus.Abstractions;
using Xunit;

namespace GuimoSoft.Bus.Tests.Abstractions
{
    public class ConsumeInformationsTests
    {
        [Fact]
        public void ConstructorFacts()
        {
            var sut = new ConsumeInformations(BusName.Kafka, ServerName.Default, "");

            sut.Headers
                .Should().BeEmpty();
        }

        [Theory]
        [InlineData(null, "value")]
        [InlineData("", "value")]
        [InlineData(" ", "value")]
        [InlineData("key", null)]
        [InlineData("key", "")]
        [InlineData("key", " ")]
        public void Given_AnInvalidKeysAndValues_When_AddHeader_Then_ThrowArgumentException(string key, string value)
        {
            var sut = new ConsumeInformations(BusName.Kafka, ServerName.Default, "");

            Assert.Throws<ArgumentException>(() => sut.AddHeader(key, value));
        }

        [Fact]
        public void Given_AnValidKeyAndValue_When_AddHeader_Then_HeaderAddedSuccessfully()
        {
            var sut = new ConsumeInformations(BusName.Kafka, ServerName.Default, "");

            sut.AddHeader("key", "value");

            sut.Headers
                .Should().HaveCount(1);

            sut.Headers["key"]
                .Should().Be("value");
        }
    }
}
