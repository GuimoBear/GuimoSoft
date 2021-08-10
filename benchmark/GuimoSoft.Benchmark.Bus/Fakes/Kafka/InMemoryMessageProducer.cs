using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Benchmark.Bus.Fakes.Interfaces;
using GuimoSoft.Core.Serialization;

namespace GuimoSoft.Benchmark.Bus.Fakes.Kafka
{
    public class InMemoryMessageProducer : IMessageProducer
    {
        private readonly IBrokerProducer _producer;

        public InMemoryMessageProducer(IBrokerProducer producer)
        {
            _producer = producer;
        }

        public Task ProduceAsync<TMessage>(string key, TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessage
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            _producer.Enqueue(key, JsonMessageSerializer.Instance.Serialize(message));
            return Task.CompletedTask;
        }
    }
}
