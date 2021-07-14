using Confluent.Kafka;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Serialization.Interfaces;

namespace GuimoSoft.Bus.Kafka.Producer
{
    internal sealed class KafkaMessageProducer : IMessageProducer, IDisposable
    {
        private readonly Lazy<IProducer<string, byte[]>> _cachedProducer;
        private readonly IMessageSerializerManager _messageSerializerManager;

        public KafkaMessageProducer(IKafkaProducerBuilder kafkaProducerBuilder, IMessageSerializerManager messageSerializerManager)
        {
            _cachedProducer = new Lazy<IProducer<string, byte[]>>(() => kafkaProducerBuilder.Build());
            _messageSerializerManager = messageSerializerManager;
        }

        public void Dispose()
        {
            if (_cachedProducer.IsValueCreated) _cachedProducer.Value.Dispose();
        }

        public async Task ProduceAsync<TMessage>(string key, TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessage
        {
            var serializer = _messageSerializerManager.GetSerializer(typeof(TMessage));
            var serializedMessage = serializer.Serialize(message);
            var topic = Attribute.GetCustomAttributes(message.GetType())
                .OfType<MessageTopicAttribute>()
                .Single()
                .Topic;

            var messageType = message.GetType().AssemblyQualifiedName;
            var producedMessage = new Message<string, byte[]>
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