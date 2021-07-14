using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;

namespace GuimoSoft.Bus.Kafka.Common
{
    public class KafkaTopicCache : IKafkaTopicCache
    {
        private readonly Lazy<ConcurrentDictionary<Type, string>> _topics;

        private readonly Lazy<ConcurrentDictionary<string, List<Type>>> _messageTypes;

        private readonly IServiceCollection _serviceCollection;

        public KafkaTopicCache(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;

            _topics = new Lazy<ConcurrentDictionary<Type, string>>(GetTopicCache, LazyThreadSafetyMode.ExecutionAndPublication);
            _messageTypes = new Lazy<ConcurrentDictionary<string, List<Type>>>(GetMessageTypes, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public string this[IMessage message]
        {
            get
            {
                if (message is null)
                    throw new ArgumentNullException(nameof(message));
                var messageType = message.GetType();

                return this[messageType];
            }
        }

        public string this[Type messageType]
        {
            get
            {
                if (_topics.Value.TryGetValue(messageType, out string topic))
                    return topic;
                throw new KeyNotFoundException($"Não existe um tópico registrado para a mensagem do tipo '{messageType.FullName}'");
            }
        }

        public IReadOnlyCollection<Type> this[string topic]
        {
            get
            {
                if (_messageTypes.Value.TryGetValue(topic, out var types))
                    return types;
                throw new KeyNotFoundException($"Não existem mensagens para o tópico '{topic}'");
            }
        }

        private ConcurrentDictionary<Type, string> GetTopicCache()
        {
            var messageTypesWithNotificationHandlers = _serviceCollection
                .Where(s => s.ServiceType.IsGenericType &&
                            s.ServiceType.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                .Select(s => s.ServiceType.GetGenericArguments()[0])
                .Where(s => s.IsGenericType &&
                            s.GetGenericTypeDefinition() == typeof(MessageNotification<>))
                .Select(s => s.GetGenericArguments()[0])
                .Where(s => typeof(IMessage).IsAssignableFrom(s))
                .Distinct();

            var result = new Dictionary<Type, string>(messageTypesWithNotificationHandlers.Count());
            foreach (var messageType in messageTypesWithNotificationHandlers)
            {
                var attribute = Attribute.GetCustomAttributes(messageType).OfType<MessageTopicAttribute>().FirstOrDefault();
                if (attribute is not null)
                    result.Add(messageType, attribute.Topic);
            }

            return new ConcurrentDictionary<Type, string>(result);
        }

        private ConcurrentDictionary<string, List<Type>> GetMessageTypes()
        {
            var messageTypesWithNotificationHandlers = _serviceCollection
                .Where(s => s.ServiceType.IsGenericType &&
                            s.ServiceType.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                .Select(s => s.ServiceType.GetGenericArguments()[0])
                .Where(s => s.IsGenericType &&
                            s.GetGenericTypeDefinition() == typeof(MessageNotification<>))
                .Select(s => s.GetGenericArguments()[0])
                .Where(s => typeof(IMessage).IsAssignableFrom(s))
                .Distinct();

            var result = new Dictionary<string, List<Type>>();
            foreach (var messageType in messageTypesWithNotificationHandlers)
            {
                var attribute = Attribute.GetCustomAttributes(messageType).OfType<MessageTopicAttribute>().FirstOrDefault();
                if (attribute is not null)
                {
                    if (result.TryGetValue(attribute.Topic, out var types))
                        types.Add(messageType);
                    else
                        result.TryAdd(attribute.Topic, new List<Type> { messageType });
                }
            }
            return new ConcurrentDictionary<string, List<Type>>(result);
        }
    }
}
