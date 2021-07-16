using GuimoSoft.Bus.Core.Logs.Interfaces;
using GuimoSoft.Bus.Kafka.Common;
using GuimoSoft.Bus.Kafka.Consumer;
using Microsoft.Extensions.Options;
using Moq;
using System;
using Xunit;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class KafkaConsumerBuilderTests
    {
        [Fact]
        public void ConstructorShouldCreateSampleConsumerBuilder()
        {
            var kafkaOptions = Options.Create(new KafkaOptions());
            var moqBusLogger = new Mock<IBusLogger>();

            var sut = new KafkaConsumerBuilder(kafkaOptions, moqBusLogger.Object);

            Assert.IsType<KafkaConsumerBuilder>(sut);
        }

        [Fact]
        public void ConstructorShouldThrowIfAnyParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new KafkaConsumerBuilder(null, null));
            Assert.Throws<ArgumentNullException>(() => new KafkaConsumerBuilder(null, Mock.Of<IBusLogger>()));
            Assert.Throws<ArgumentNullException>(() => new KafkaConsumerBuilder(Options.Create(new KafkaOptions()), null));
        }

        [Fact]
        public void BuildShouldReturnNonNullConsumer()
        {
            var kafkaOptions = Options.Create(new KafkaOptions
            {
                KafkaBootstrapServers = "kafka-bootstrap-servers",
                ConsumerGroupId = "test-group-id",
                AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest
            });

            var sut = new KafkaConsumerBuilder(kafkaOptions, Mock.Of<IBusLogger>());

            var consumer = sut.Build();

            Assert.NotNull(consumer);

            sut = new KafkaConsumerBuilder(kafkaOptions, Mock.Of<IBusLogger>());

            consumer = sut.Build();

            Assert.NotNull(consumer);
        }
    }
}