using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System;
using GuimoSoft.MessageBroker.Kafka.Common;

namespace GuimoSoft.MessageBroker.Kafka.Consumer
{
    public class KafkaConsumerBuilder : IKafkaConsumerBuilder
    {
        private readonly KafkaOptions _kafkaOptions;

        public KafkaConsumerBuilder(IOptions<KafkaOptions> kafkaOptions)
        {
            _kafkaOptions = kafkaOptions?.Value
                            ?? throw new ArgumentNullException(nameof(kafkaOptions));
        }

        public IConsumer<string, string> Build()
        {
            var consumerConfig = new ConsumerConfig()
            {
                GroupId = _kafkaOptions.ConsumerGroupId,
                BootstrapServers = _kafkaOptions.KafkaBootstrapServers,
                AutoOffsetReset = _kafkaOptions.AutoOffsetReset
            };

            var consumerBuilder = new ConsumerBuilder<string, string>(consumerConfig);

            return consumerBuilder.Build();
        }
    }
}