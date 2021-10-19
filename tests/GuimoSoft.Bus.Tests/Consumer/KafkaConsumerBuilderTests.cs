using Confluent.Kafka;
using FluentAssertions;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Kafka.Consumer;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class KafkaConsumerBuilderTests
    {
        public static readonly IEnumerable<object[]> ConstructorInvalidData
            = new List<object[]>
            {
                new object[] { null, null },
                new object[] { null, Mock.Of<IBusLogDispatcher>() },
                new object[] { Mock.Of<IBusOptionsDictionary<ConsumerConfig>>(), null }
            };

        [Fact]
        public void ConstructorShouldCreateSampleConsumerBuilder()
        {
            var sut = new KafkaConsumerBuilder(Mock.Of<IBusOptionsDictionary<ConsumerConfig>>(), Mock.Of<IBusLogDispatcher>());
            Assert.IsType<KafkaConsumerBuilder>(sut);
        }

        [Theory]
        [MemberData(nameof(ConstructorInvalidData))]
        internal void Given_DefaultParameters_When_Construct_Then_ThrowArgumentNullException(IBusOptionsDictionary<ConsumerConfig> options, IBusLogDispatcher logger)
        {
            Assert.Throws<ArgumentNullException>(() => new KafkaConsumerBuilder(options, logger));
        }

        [Fact]
        public void Given_AnEmptyBusOptionsDictionary_When_Build_Then_ThrowKeyNotFoundException()
        {
            var sut = new KafkaConsumerBuilder(Mock.Of<IBusOptionsDictionary<ConsumerConfig>>(), Mock.Of<IBusLogDispatcher>());

            Assert.Throws<KeyNotFoundException>(() => sut.Build(ServerName.Default));
        }

        [Fact]
        public void Given_AnFilledBusOptionsDictionary_When_Build_Then_ReturnAnProducer()
        {
            var kafkaOptions = new ConsumerConfig() { GroupId = "fake-group-id" };

            var moqDictionary = new Mock<IBusOptionsDictionary<ConsumerConfig>>();
            moqDictionary
                .Setup(x => x.TryGetValue(ServerName.Default, out kafkaOptions))
                .Returns(true);

            var sut = new KafkaConsumerBuilder(moqDictionary.Object, Mock.Of<IBusLogDispatcher>());

            sut.Build(ServerName.Default)
                .Should().NotBeNull();

            moqDictionary
                .Verify(x => x.TryGetValue(ServerName.Default, out kafkaOptions), Times.Once);
        }
    }
}