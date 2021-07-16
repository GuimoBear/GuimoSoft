using Confluent.Kafka;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Bus.Core.Logs.Interfaces;
using GuimoSoft.Bus.Kafka.Common;
using GuimoSoft.Core.Serialization.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    internal class KafkaTopicMessageConsumer : IKafkaTopicMessageConsumer
    {
        private readonly IKafkaConsumerBuilder _kafkaConsumerBuilder;
        private readonly IServiceProvider _serviceProvider;
        private readonly IKafkaTopicCache _cache;
        private readonly IMessageMiddlewareExecutorProvider _middlewareManager;
        private readonly IMessageSerializerManager _messageSerializerManager;
        private readonly IBusLogger _busLogger;

        public KafkaTopicMessageConsumer(IKafkaConsumerBuilder kafkaConsumerBuilder, IServiceProvider serviceProvider, IKafkaTopicCache cache, IMessageMiddlewareExecutorProvider middlewareManager, IMessageSerializerManager messageSerializerManager, IBusLogger busLogger)
        {
            _kafkaConsumerBuilder = kafkaConsumerBuilder;
            _serviceProvider = serviceProvider;
            _cache = cache;
            _middlewareManager = middlewareManager;
            _messageSerializerManager = messageSerializerManager;
            _busLogger = busLogger ?? throw new ArgumentNullException(nameof(busLogger));
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
                    var exceptionMessage = new ExceptionMessage(
                        $"Houve um erro ao consumir a mensagem do t�pico {topic}",
                        ex is OperationCanceledException ? LogLevel.Warning : LogLevel.Error)
                    {
                        Exception = ex
                    };
                    exceptionMessage.Data.Add(nameof(topic), topic);
                    _busLogger.ExceptionAsync(exceptionMessage).ConfigureAwait(false);
                    if (ex is OperationCanceledException)
                        break;
                }
            }
        }

        private void ProcessMessage(ref string topic, ConsumeResult<string, byte[]> consumeResult)
        {
            var messageTypes = _cache[topic];
            if (messageTypes.Count > 0)
            {
                var timerMessageProcess = new Stopwatch();
                timerMessageProcess.Start();
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
                        var exceptionMessage = new ExceptionMessage(
                            $"Houve um erro ao consumir a mensagem do tipo {messageType.FullName} t�pico {topic}",
                            ex is OperationCanceledException ? LogLevel.Warning : LogLevel.Error)
                        {
                            Exception = ex
                        };
                        exceptionMessage.Data.Add(nameof(messageType), messageType.FullName);
                        exceptionMessage.Data.Add(nameof(topic), topic);
                        _busLogger.ExceptionAsync(exceptionMessage).ConfigureAwait(false);
                    }
                    finally
                    {
                        timerMessageProcess.Stop();
                        var log = Core.Logs.LogMessage.Trace($"Finalizado o processamento da mensagem do tipo {messageType.FullName}");
                        log.Data.Add(nameof(topic), topic);
                        log.Data.Add(nameof(messageType), messageType.FullName);
                        log.Data.Add("milisegundos", timerMessageProcess.ElapsedMilliseconds);
                        _busLogger.LogAsync(log).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}