using Microsoft.Extensions.Options;
using System;
using GuimoSoft.MessageBroker.Kafka.Common;
using GuimoSoft.MessageBroker.Kafka.Consumer;
using Xunit;

namespace GuimoSoft.MessageBroker.Kafka.Tests.Consumer
{
    public class KafkaConsumerBuilderTests
    {
        [Fact]
        public void ConstructorShouldCreateSampleConsumerBuilder()
        {
            var kafkaOptions = Options.Create(new KafkaOptions());

            var sut = new KafkaConsumerBuilder(kafkaOptions);

            Assert.IsType<KafkaConsumerBuilder>(sut);
        }

        [Fact]
        public void ConstructorShouldThrowIfOptionsIsNull()
        {
            IOptions<KafkaOptions> kafkaOptions = null;

            Assert.Throws<ArgumentNullException>(() => new KafkaConsumerBuilder(kafkaOptions));
        }

        [Fact]
        public void BuildShouldReturnNonNullConsumer()
        {
            var kafkaOptions = Options.Create(new KafkaOptions
            {
                KafkaBootstrapServers = "kafka-bootstrap",
                ConsumerGroupId = "test-group-id",
                AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest
            });

            var sut = new KafkaConsumerBuilder(kafkaOptions);

            var consumer = sut.Build();

            Assert.NotNull(consumer);
        }
    }
}