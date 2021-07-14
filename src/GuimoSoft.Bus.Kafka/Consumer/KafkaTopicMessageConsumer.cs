using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Kafka.Common;
using GuimoSoft.Serialization.Interfaces;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    internal class KafkaTopicMessageConsumer : IKafkaTopicMessageConsumer
    {
        private readonly IKafkaConsumerBuilder _kafkaConsumerBuilder;
        private readonly IServiceProvider _serviceProvider;
        private readonly IKafkaTopicCache _cache;
        private readonly IMessageMiddlewareExecutorProvider _middlewareManager;
        private readonly IMessageSerializerManager _messageSerializerManager;

        public KafkaTopicMessageConsumer(IKafkaConsumerBuilder kafkaConsumerBuilder, IServiceProvider serviceProvider, IKafkaTopicCache cache, IMessageMiddlewareExecutorProvider middlewareManager, IMessageSerializerManager messageSerializerManager)
        {
            _kafkaConsumerBuilder = kafkaConsumerBuilder;
            _serviceProvider = serviceProvider;
            _cache = cache;
            _middlewareManager = middlewareManager;
            _messageSerializerManager = messageSerializerManager;
        }

        public void ConsumeUntilCancellationIsRequested(string topic, CancellationToken cancellationToken)
        {
            using var consumer = _kafkaConsumerBuilder.Build();
            consumer.Subscribe(topic);
            ListenUntilCancellationIsRequested(ref topic, consumer, ref cancellationToken);
            consumer.Close();
        }

        private void ListenUntilCancellationIsRequested(ref string topic, IConsumer<string, byte[]> consumer, ref CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    ProcessMessage(ref topic, consumeResult);
                }
                catch (Exception ex)
                {
                    if (ex is not OperationCanceledException)
                        Logger.LogWarning($"Falha ao consumir a mensagem do tópico '{topic}' no Kafka: {ex.Message}");
                    else
                        break;
                }
            }
        }

        private void ProcessMessage(ref string topic, ConsumeResult<string, byte[]> consumeResult)
        {
            var messageTypes = _cache[topic];
            if (messageTypes.Count > 0)
            {
                foreach (var messageType in messageTypes)
                {
                    try
                    {
                        var serializer = _messageSerializerManager.GetSerializer(messageType);
                        var deserializedMessage = serializer.Deserialize(messageType, consumeResult.Message.Value);
                        using var scope = _serviceProvider.CreateScope();
                        var pipeline = _middlewareManager.GetPipeline(messageType);
                        pipeline.Execute(messageType, deserializedMessage, scope.ServiceProvider).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        if (ex is not OperationCanceledException)
                            Logger.LogError($"Houve um erro ao processar a mensagem do tipo '{messageType.Name}' do tópico '{topic}'", ex);
                    }
                }
            }
        }
    }
}