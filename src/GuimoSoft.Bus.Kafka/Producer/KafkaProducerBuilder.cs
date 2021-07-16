using Confluent.Kafka;
using GuimoSoft.Bus.Core.Logs.Interfaces;
using GuimoSoft.Bus.Kafka.Common;
using Microsoft.Extensions.Options;
using System;

namespace GuimoSoft.Bus.Kafka.Producer
{
    public class KafkaProducerBuilder : IKafkaProducerBuilder
    {
        private readonly KafkaOptions _kafkaOptions;
        private readonly IBusLogger _busLogger;

        public KafkaProducerBuilder(IOptions<KafkaOptions> kafkaOptions, IBusLogger busLogger)
        {
            _kafkaOptions = kafkaOptions?.Value
                            ?? throw new ArgumentNullException(nameof(kafkaOptions));
            _busLogger = busLogger ?? throw new ArgumentNullException(nameof(busLogger));
        }

        public IProducer<string, byte[]> Build()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _kafkaOptions.KafkaBootstrapServers,
                Acks = _kafkaOptions.Acks
            };

            var producerBuilder = new ProducerBuilder<string, byte[]>(config);

            producerBuilder.SetLogHandler((_, kafkaLogMessage) => _busLogger.LogAsync(KafkaLogConverter.Cast(kafkaLogMessage)).ConfigureAwait(false));
            producerBuilder.SetErrorHandler((_, kafkaError) => _busLogger.ExceptionAsync(KafkaLogConverter.Cast(kafkaError)).ConfigureAwait(false));

            return producerBuilder.Build();
        }
    }
}