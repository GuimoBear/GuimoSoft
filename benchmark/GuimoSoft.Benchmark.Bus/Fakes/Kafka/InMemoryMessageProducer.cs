using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Benchmark.Bus.Fakes.Interfaces;
using GuimoSoft.Core.Serialization;

namespace GuimoSoft.Benchmark.Bus.Fakes.Kafka
{
    public class InMemoryEventProducer : IEventBus
    {
        private readonly IBrokerProducer _producer;

        public InMemoryEventProducer(IBrokerProducer producer)
        {
            _producer = producer;
        }

        public Task Publish<TEvent>(string key, TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));
            _producer.Enqueue(key, JsonEventSerializer.Instance.Serialize(@event));
            return Task.CompletedTask;
        }
    }
}
