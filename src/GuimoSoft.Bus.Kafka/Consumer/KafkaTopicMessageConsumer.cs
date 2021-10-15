using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Core.Serialization.Interfaces;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    internal class KafkaTopicEventConsumer : IKafkaTopicEventConsumer
    {
        private readonly IKafkaConsumerBuilder _kafkaConsumerBuilder;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventTypeCache _cache;
        private readonly IEventMiddlewareExecutorProvider _middlewareManager;
        private readonly IBusSerializerManager _busSerializerManager;
        private readonly IBusLogDispatcher _log;

        private IEnumerable<EventTypeResolver> _eventTypeResolvers;

        public KafkaTopicEventConsumer(IKafkaConsumerBuilder kafkaConsumerBuilder, IServiceProvider serviceProvider, IEventTypeCache cache, IEventMiddlewareExecutorProvider middlewareManager, IBusSerializerManager busSerializerManager, IBusLogDispatcher log)
        {
            _kafkaConsumerBuilder = kafkaConsumerBuilder ?? throw new ArgumentNullException(nameof(kafkaConsumerBuilder));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _middlewareManager = middlewareManager ?? throw new ArgumentNullException(nameof(middlewareManager));
            _busSerializerManager = busSerializerManager ?? throw new ArgumentNullException(nameof(busSerializerManager));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void ConsumeUntilCancellationIsRequested(Enum @switch, string topic, CancellationToken cancellationToken)
        {
            using var consumer = _kafkaConsumerBuilder.Build(@switch);
            InitializeCachedObjects(@switch, topic);
            consumer.Subscribe(topic);
            ListenUntilCancellationIsRequested(@switch, ref topic, consumer, ref cancellationToken);
            consumer.Close();
        }

        private void ListenUntilCancellationIsRequested(Enum @switch, ref string topic, IConsumer<string, byte[]> consumer, ref CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    ProcessEvent(@switch, ref topic, consumeResult, cancellationToken);
                }
                catch (Exception ex)
                {
                    _log
                        .FromBus(BusName.Kafka).AndSwitch(@switch).AndFinality(Finality.Consume)
                        .WhileListening().TheEndpoint(topic)
                        .Write().Message($"Houve um erro ao consumir a mensagem do tópico {topic}")
                        .With(BusLogLevel.Warning)
                        .Publish().AnException(ex, cancellationToken);
                    break;
                }
            }
        }

        private void InitializeCachedObjects(Enum @switch, string topic)
        {
            var eventTypes = _cache.Get(BusName.Kafka, Finality.Consume, @switch, topic);
            var eventTypeResolvers = new List<EventTypeResolver>();
            foreach (var eventType in eventTypes)
            {
                var serializer = _busSerializerManager.GetSerializer(BusName.Kafka, Finality.Consume, @switch, eventType);
                var pipeline = _middlewareManager.GetPipeline(BusName.Kafka, @switch, eventType);
                eventTypeResolvers.Add(new(eventType, serializer, pipeline));
            }
            _eventTypeResolvers = eventTypeResolvers;
        }

        private void ProcessEvent(Enum @switch, ref string topic, ConsumeResult<string, byte[]> consumeResult, CancellationToken cancellationToken)
        {
            foreach (var eventTypeResolver in _eventTypeResolvers)
                ProcessEventType(@switch, topic, consumeResult, eventTypeResolver, cancellationToken);
        }

        private void ProcessEventType(Enum @switch, string topic, ConsumeResult<string, byte[]> consumeResult, EventTypeResolver eventTypeResolver, CancellationToken cancellationToken)
        {
            try
            {
                if (!TryDeserialize(eventTypeResolver, consumeResult.Message.Value, out var deserializedEvent))
                {
                    _log
                        .FromBus(BusName.Kafka).AndSwitch(@switch).AndFinality(Finality.Consume)
                        .AfterReceived().TheEvent(eventTypeResolver.EventType, deserializedEvent).FromEndpoint(topic)
                        .Write().Message($"Houve um erro ao deserializar a mensagem do tipo {eventTypeResolver.EventType.Name} após receber uma mensagem do tópico {topic}")
                        .With(BusLogLevel.Error)
                        .Publish().AnLog(cancellationToken);
                    return;
                }

                ExecutePipeline(@switch, topic, consumeResult.Message.Headers, eventTypeResolver, deserializedEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _log
                    .FromBus(BusName.Kafka).AndSwitch(@switch).AndFinality(Finality.Consume)
                    .WhileListening().TheEndpoint(topic)
                    .Write().Message($"Houve um erro ao deserializar a mensagem do tipo {eventTypeResolver.EventType.Name} após receber uma mensagem do tópico {topic}")
                    .With(BusLogLevel.Error)
                    .Publish().AnException(ex, cancellationToken);
            }
        }

        private static bool TryDeserialize(EventTypeResolver eventTypeResolver, byte[] content, out object deserializedEvent)
        {
            deserializedEvent = eventTypeResolver.Serializer.Deserialize(eventTypeResolver.EventType, content);
            return deserializedEvent is not null;
        }

        private void ExecutePipeline(Enum @switch, string topic, Headers headers, EventTypeResolver eventTypeResolver, object deserializedEvent, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var informations = GetInformations(@switch, ref topic, headers);

                eventTypeResolver.Pipeline.Execute(deserializedEvent, scope.ServiceProvider, informations, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _log
                    .FromBus(BusName.Kafka).AndSwitch(@switch).AndFinality(Finality.Consume)
                    .AfterReceived().TheEvent(deserializedEvent).FromEndpoint(topic)
                    .Write().Message($"Houve um erro ao executar a pipeline do tipo {eventTypeResolver.EventType.Name} após receber uma mensagem do tópico {topic}")
                    .With(ex is OperationCanceledException ? BusLogLevel.Warning : BusLogLevel.Error)
                    .Publish().AnException(ex, cancellationToken);
            }
        }

        private static ConsumeInformations GetInformations(Enum @switch, ref string topic, Headers headers)
        {
            var informations = new ConsumeInformations(BusName.Kafka, @switch, topic);
            if (headers is not null)
            {
                foreach (var header in headers)
                    informations.AddHeader(header.Key, Encoding.UTF8.GetString(header.GetValueBytes()));
            }
            return informations;
        }

        private class EventTypeResolver
        {
            public Type EventType { get; }
            public IDefaultSerializer Serializer { get; }
            public Pipeline Pipeline { get; }

            public EventTypeResolver(Type eventType, IDefaultSerializer serializer, Pipeline pipeline)
            {
                EventType = eventType;
                Serializer = serializer;
                Pipeline = pipeline;
            }
        }
    }
}