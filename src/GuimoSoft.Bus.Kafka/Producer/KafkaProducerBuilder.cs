using System;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using GuimoSoft.Bus.Kafka.Common;

namespace GuimoSoft.Bus.Kafka.Producer
{
    public class KafkaProducerBuilder : IKafkaProducerBuilder
    {
        private readonly KafkaOptions _kafkaOptions;
        private readonly KafkaEventsOptions _kafkaConsumerEventsOptions;

        public KafkaProducerBuilder(IOptions<KafkaOptions> kafkaOptions, IOptions<KafkaEventsOptions> kafkaConsumerEventsOptions)
        {
            _kafkaOptions = kafkaOptions?.Value
                            ?? throw new ArgumentNullException(nameof(kafkaOptions));
            _kafkaConsumerEventsOptions = kafkaConsumerEventsOptions?.Value;
        }

        public IProducer<string, byte[]> Build()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _kafkaOptions.KafkaBootstrapServers,
                Acks = _kafkaOptions.Acks
            };

            var producerBuilder = new ProducerBuilder<string, byte[]>(config);

            if (_kafkaConsumerEventsOptions is not null)
                _kafkaConsumerEventsOptions.SetEvents(producerBuilder);

            return producerBuilder.Build();
        }
    }
}