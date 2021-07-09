using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using System.Threading;
using GuimoSoft.MessageBroker.Abstractions.Consumer;
using GuimoSoft.MessageBroker.Kafka.Common;

namespace GuimoSoft.MessageBroker.Kafka.Consumer
{
    public class KafkaTopicMessageConsumer : IKafkaTopicMessageConsumer
    {
        private readonly IKafkaConsumerBuilder _kafkaConsumerBuilder;
        private readonly IServiceProvider _serviceProvider;
        private readonly IKafkaTopicCache _cache;
        private readonly IMessageMiddlereExecutorProvider _middlewareManager;

        public KafkaTopicMessageConsumer(IKafkaConsumerBuilder kafkaConsumerBuilder, IServiceProvider serviceProvider, IKafkaTopicCache cache, IMessageMiddlereExecutorProvider middlewareManager)
        {
            _kafkaConsumerBuilder = kafkaConsumerBuilder;
            _serviceProvider = serviceProvider;
            _cache = cache;
            _middlewareManager = middlewareManager;
        }

        public void ConsumeUntilCancellationIsRequested(string topic, CancellationToken cancellationToken)
        {
            using var consumer = _kafkaConsumerBuilder.Build();
            consumer.Subscribe(topic);
            ListenUntilCancellationIsRequested(ref topic, consumer, ref cancellationToken);
            consumer.Close();
        }

        private void ListenUntilCancellationIsRequested(ref string topic, IConsumer<string, string> consumer, ref CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    ProcessMessage(ref topic, consumeResult, cancellationToken);
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

        private void ProcessMessage(ref string topic, ConsumeResult<string, string> consumeResult, CancellationToken cancellationToken)
        {
            var messageTypes = _cache[topic];
            if (messageTypes.Count > 0)
            {
                foreach (var messageType in messageTypes)
                {
                    try
                    {
                        var (deserializedMessage, messageNotification) = DeserializeAndCreateMessageNotification(consumeResult.Message.Value, messageType);
                        using var scope = _serviceProvider.CreateScope();
                        var pipeline = _middlewareManager.GetPipeline(messageType);
                        pipeline.Execute(messageType, deserializedMessage, scope.ServiceProvider, async context => 
                        {
                            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                            await mediator.Publish(messageNotification, cancellationToken).ConfigureAwait(false);

                        }).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        if (ex is not OperationCanceledException)
                            Logger.LogError($"Houve um erro ao processar a mensagem do tipo '{messageType.Name}' do tópico '{topic}'", ex);
                    }
                }
            }
        }

        private static (object deserializedMessage, object messageNotification) DeserializeAndCreateMessageNotification(string message, Type messageType)
        {
            var deserializedMessage = JsonSerializer.Deserialize(message, messageType);
            var messageNotificationType = typeof(MessageNotification<>).MakeGenericType(messageType);
            return (deserializedMessage, Activator.CreateInstance(messageNotificationType, deserializedMessage));
        }
    }
}