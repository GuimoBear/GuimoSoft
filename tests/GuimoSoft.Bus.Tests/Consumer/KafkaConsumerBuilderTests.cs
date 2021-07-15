using Microsoft.Extensions.Options;
using System;
using GuimoSoft.Bus.Kafka.Common;
using GuimoSoft.Bus.Kafka.Consumer;
using Xunit;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class KafkaConsumerBuilderTests
    {
        [Fact]
        public void ConstructorShouldCreateSampleConsumerBuilder()
        {
            var kafkaOptions = Options.Create(new KafkaOptions());
            var kafkaEventsOptions = Options.Create(new KafkaEventsOptions());

            var sut = new KafkaConsumerBuilder(kafkaOptions, kafkaEventsOptions);

            Assert.IsType<KafkaConsumerBuilder>(sut);
        }

        [Fact]
        public void ConstructorShouldThrowIfOptionsIsNull()
        {
            IOptions<KafkaOptions> kafkaOptions = null;

            Assert.Throws<ArgumentNullException>(() => new KafkaConsumerBuilder(kafkaOptions, null));
        }

        [Fact]
        public void BuildShouldReturnNonNullConsumer()
        {
            var kafkaOptions = Options.Create(new KafkaOptions
            {
                KafkaBootstrapServers = "kafka-bootstrap",
                ConsumerGroupId = "test-group-id",
                AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest, 
                Acks = Confluent.Kafka.Acks.All
            });

            var sut = new KafkaConsumerBuilder(kafkaOptions, null);

            var consumer = sut.Build();

            Assert.NotNull(consumer);
        }
    }
}