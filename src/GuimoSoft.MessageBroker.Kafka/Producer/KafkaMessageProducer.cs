using Confluent.Kafka;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.MessageBroker.Abstractions;

namespace GuimoSoft.MessageBroker.Kafka.Producer
{
    public sealed class KafkaMessageProducer : IMessageProducer, IDisposable
    {
        private readonly Lazy<IProducer<string, string>> _cachedProducer;

        public KafkaMessageProducer(IKafkaProducerBuilder kafkaProducerBuilder)
        {
            _cachedProducer = new Lazy<IProducer<string, string>>(() => kafkaProducerBuilder.Build());
        }

        public void Dispose()
        {
            if (_cachedProducer.IsValueCreated) _cachedProducer.Value.Dispose();
        }

        public async Task ProduceAsync<TMessage>(string key, TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessage
        {
            var serializedMessage = JsonSerializer.Serialize(message);
            var topic = Attribute.GetCustomAttributes(message.GetType())
                .OfType<MessageTopicAttribute>()
                .Single()
                .Topic;

            var messageType = message.GetType().AssemblyQualifiedName;
            var producedMessage = new Message<string, string>
            {
                Key = key,
                Value = serializedMessage,
                Headers = new Headers
                {
                    {"message-type", Encoding.UTF8.GetBytes(messageType)}
                }
            };

            await _cachedProducer.Value.ProduceAsync(topic, producedMessage, cancellationToken);
        }
    }
}