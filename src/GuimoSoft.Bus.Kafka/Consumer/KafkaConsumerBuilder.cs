using Confluent.Kafka;
using GuimoSoft.Bus.Core.Logs.Interfaces;
using GuimoSoft.Bus.Kafka.Common;
using Microsoft.Extensions.Options;
using System;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    public class KafkaConsumerBuilder : IKafkaConsumerBuilder
    {
        private readonly KafkaOptions _kafkaOptions;
        private readonly IBusLogger _busLogger;

        public KafkaConsumerBuilder(IOptions<KafkaOptions> kafkaOptions, IBusLogger busLogger)
        {
            _kafkaOptions = kafkaOptions?.Value
                            ?? throw new ArgumentNullException(nameof(kafkaOptions));
            _busLogger = busLogger ?? throw new ArgumentNullException(nameof(busLogger));
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

            consumerBuilder.SetLogHandler((_, kafkaLogMessage) => _busLogger.LogAsync(KafkaLogConverter.Cast(kafkaLogMessage)).ConfigureAwait(false));
            consumerBuilder.SetErrorHandler((_, kafkaError) => _busLogger.ExceptionAsync(KafkaLogConverter.Cast(kafkaError)).ConfigureAwait(false));

            return consumerBuilder.Build();
        }
    }
}