using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    public class KafkaEventConsumerManager : IKafkaEventConsumerManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventTypeCache _cache;

        private bool _started = false;
        private readonly IList<Task> _tasks;

        public KafkaEventConsumerManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _cache = _serviceProvider.GetRequiredService<IEventTypeCache>();

            _tasks = new List<Task>();
        }

        public void StartConsumers(CancellationToken cancellationToken)
        {
            if (!_started)
            {
                foreach (var (@switch, topic) in _cache.GetSwitchers(BusName.Kafka, Finality.Consume)
                                                       .SelectMany(@switch => _cache.GetEndpoints(BusName.Kafka, Finality.Consume, @switch)
                                                                                    .Select(topic => new KeyValuePair<Enum, string>(@switch, topic))))
                {
                    var kafkaTopicEventConsumer = _serviceProvider.GetRequiredService<IKafkaTopicEventConsumer>();

                    _tasks.Add(Task.Factory.StartNew(() => kafkaTopicEventConsumer.ConsumeUntilCancellationIsRequested(@switch, topic, cancellationToken), cancellationToken));
                }
                _started = true;
            }
            if (_tasks.Count > 0)
            {
                Task.WaitAll(_tasks.ToArray(), cancellationToken);
                _tasks.Clear();
            }
        }
    }
}