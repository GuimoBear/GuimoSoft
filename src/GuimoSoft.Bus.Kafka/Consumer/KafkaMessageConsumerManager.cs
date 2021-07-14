using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Kafka.Common;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    public class KafkaMessageConsumerManager : IKafkaMessageConsumerManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceCollection _services;
        private readonly IKafkaTopicCache _cache;

        private bool _started = false;
        private readonly IList<Task> _tasks;

        public KafkaMessageConsumerManager(IServiceProvider serviceProvider, IServiceCollection services)
        {
            _serviceProvider = serviceProvider;
            _services = services;
            _cache = _serviceProvider.GetRequiredService<IKafkaTopicCache>();

            _tasks = new List<Task>();
        }

        public void StartConsumers(CancellationToken cancellationToken)
        {
            if (!_started)
            {
                var topicsWithNotificationHandlers = GetTopicsWithNotificationHandlers();

                foreach (var topic in topicsWithNotificationHandlers)
                {
                    var kafkaTopicMessageConsumer = _serviceProvider.GetRequiredService<IKafkaTopicMessageConsumer>();

                    _tasks.Add(Task.Factory.StartNew(() => kafkaTopicMessageConsumer.ConsumeUntilCancellationIsRequested(topic, cancellationToken), cancellationToken));
                }
                _started = true;
            }
            if (_tasks.Count > 0)
            {
                Task.WaitAll(_tasks.ToArray(), cancellationToken);
                _tasks.Clear();
            }
        }

        private IEnumerable<string> GetTopicsWithNotificationHandlers()
        {
            var messageTypesWithNotificationHandlers = _services
                .Where(s => s.ServiceType.IsGenericType &&
                            s.ServiceType.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                .Select(s => s.ServiceType.GetGenericArguments()[0])
                .Where(s => s.IsGenericType &&
                            s.GetGenericTypeDefinition() == typeof(MessageNotification<>))
                .Select(s => s.GetGenericArguments()[0])
                .Where(s => typeof(IMessage).IsAssignableFrom(s))
                .Distinct();

            foreach (var type in messageTypesWithNotificationHandlers)
                yield return _cache[type];
        }
    }
}