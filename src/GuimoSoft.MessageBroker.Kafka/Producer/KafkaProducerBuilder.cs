using System;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using GuimoSoft.MessageBroker.Kafka.Common;

namespace GuimoSoft.MessageBroker.Kafka.Producer
{
    public class KafkaProducerBuilder : IKafkaProducerBuilder
    {
        private readonly KafkaOptions _kafkaOptions;

        public KafkaProducerBuilder(IOptions<KafkaOptions> producerWorkerOptions)
        {
            _kafkaOptions = producerWorkerOptions?.Value ??
                            throw new ArgumentNullException(nameof(producerWorkerOptions));
        }

        public IProducer<string, string> Build()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _kafkaOptions.KafkaBootstrapServers
            };

            var producerBuilder = new ProducerBuilder<string, string>(config);

            return producerBuilder.Build();
        }
    }
}