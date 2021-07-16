using GuimoSoft.Bus.Core.Logs.Interfaces;
using GuimoSoft.Bus.Kafka.Common;
using GuimoSoft.Bus.Kafka.Producer;
using Microsoft.Extensions.Options;
using Moq;
using System;
using Xunit;

namespace GuimoSoft.Bus.Tests.Producer
{
    public class KafkaProducerBuilderTests
    {
        [Fact]
        public void ConstructorShouldCreateSampleProducerBuilder()
        {
            var kafkaOptions = Options.Create(new KafkaOptions());

            var sut = new KafkaProducerBuilder(kafkaOptions, Mock.Of<IBusLogger>());

            Assert.IsType<KafkaProducerBuilder>(sut);
        }

        [Fact]
        public void ConstructorShouldThrowIfAnyParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new KafkaProducerBuilder(null, null));
            Assert.Throws<ArgumentNullException>(() => new KafkaProducerBuilder(null, Mock.Of<IBusLogger>()));
            Assert.Throws<ArgumentNullException>(() => new KafkaProducerBuilder(Options.Create(new KafkaOptions()), null));
        }

        [Fact]
        public void BuildShouldReturnNonNullProducer()
        {
            var kafkaOptions = Options.Create(new KafkaOptions());

            var sut = new KafkaProducerBuilder(kafkaOptions, Mock.Of<IBusLogger>());

            var producer = sut.Build();

            Assert.NotNull(producer);

            sut = new KafkaProducerBuilder(kafkaOptions, Mock.Of<IBusLogger>());

            producer = sut.Build();

            Assert.NotNull(producer);
        }
    }
}