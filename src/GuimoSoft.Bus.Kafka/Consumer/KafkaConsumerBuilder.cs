using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System;
using GuimoSoft.Bus.Kafka.Common;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    public class KafkaConsumerBuilder : IKafkaConsumerBuilder
    {
        private readonly KafkaOptions _kafkaOptions;
        private readonly KafkaEventsOptions _kafkaConsumerEventsOptions;

        public KafkaConsumerBuilder(IOptions<KafkaOptions> kafkaOptions, IOptions<KafkaEventsOptions> kafkaConsumerEventsOptions)
        {
            _kafkaOptions = kafkaOptions?.Value
                            ?? throw new ArgumentNullException(nameof(kafkaOptions));
            _kafkaConsumerEventsOptions = kafkaConsumerEventsOptions?.Value;
        }

        public IConsumer<string, byte[]> Build()
        {
            var consumerConfig = new ConsumerConfig()
            {
                GroupId = _kafkaOptions.ConsumerGroupId,
                BootstrapServers = _kafkaOptions.KafkaBootstrapServers,
                AutoOffsetReset = _kafkaOptions.AutoOffsetReset,
                Acks = _kafkaOptions.Acks
            };

            var consumerBuilder = new ConsumerBuilder<string, byte[]>(consumerConfig); 
            
            if (_kafkaConsumerEventsOptions is not null)
                _kafkaConsumerEventsOptions.SetEvents(consumerBuilder);

            return consumerBuilder.Build();
        }
    }
}