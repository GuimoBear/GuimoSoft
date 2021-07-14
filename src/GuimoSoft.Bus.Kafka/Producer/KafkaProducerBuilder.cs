using System;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using GuimoSoft.Bus.Kafka.Common;

namespace GuimoSoft.Bus.Kafka.Producer
{
    public class KafkaProducerBuilder : IKafkaProducerBuilder
    {
        private readonly KafkaOptions _kafkaOptions;

        public KafkaProducerBuilder(IOptions<KafkaOptions> producerWorkerOptions)
        {
            _kafkaOptions = producerWorkerOptions?.Value ??
                            throw new ArgumentNullException(nameof(producerWorkerOptions));
        }

        public IProducer<string, byte[]> Build()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _kafkaOptions.KafkaBootstrapServers
            };

            var producerBuilder = new ProducerBuilder<string, byte[]>(config);

            return producerBuilder.Build();
        }
    }
}