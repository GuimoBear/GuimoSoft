using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;

namespace GuimoSoft.Bus.Kafka.Producer
{
    internal sealed class KafkaEventProducer : IBusEventDispatcher, IDisposable
    {
        private readonly object _lock = new();

        private readonly IKafkaProducerBuilder _kafkaProducerBuilder;
        private readonly IBusSerializerManager _busSerializerManager;
        private readonly IDictionary<Enum, IProducer<string, byte[]>> _cachedProducers;

        public KafkaEventProducer(IKafkaProducerBuilder kafkaProducerBuilder, IBusSerializerManager busSerializerManager)
        {
            _kafkaProducerBuilder = kafkaProducerBuilder ?? throw new ArgumentNullException(nameof(kafkaProducerBuilder));
            _busSerializerManager = busSerializerManager ?? throw new ArgumentNullException(nameof(busSerializerManager));
            _cachedProducers = new Dictionary<Enum, IProducer<string, byte[]>>();
        }

        public void Dispose()
        {
            foreach (var (_, producer) in _cachedProducers)
                producer.Dispose();
        }

        public async Task Dispatch<TEvent>(string key, TEvent @event, Enum @switch, string endpoint, CancellationToken cancellationToken = default) where TEvent : IEvent
        {
            ValidateParameters(key, @event, endpoint);
            var serializer = _busSerializerManager.GetSerializer(BusName.Kafka, Finality.Produce, @switch, typeof(TEvent));
            var serializedEvent = serializer.Serialize(@event);

            var eventType = @event.GetType().AssemblyQualifiedName;
            var producedEvent = new Message<string, byte[]>
            {
                Key = key,
                Value = serializedEvent,
                Headers = new Headers
                {
                    {"event-type", Encoding.UTF8.GetBytes(eventType)}
                }
            };

            await GetProducer(@switch).ProduceAsync(endpoint, producedEvent, cancellationToken);
        }

        private static void ValidateParameters<TEvent>(string key, TEvent @event, string endpoint) where TEvent : IEvent
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("� necessário informar uma chave para enviar a mensagem", nameof(key));
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("� necessário informar um endpoint para enviar a mensagem", nameof(endpoint));
        }

        private IProducer<string, byte[]> GetProducer(Enum @switch)
        {
            lock (_lock)
            {
                if (!_cachedProducers.TryGetValue(@switch, out var producer))
                {
                    producer = _kafkaProducerBuilder.Build(@switch);
                    _cachedProducers.Add(@switch, producer);
                }
                return producer;
            }
        }
    }
}