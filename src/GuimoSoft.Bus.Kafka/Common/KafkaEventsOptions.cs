using Confluent.Kafka;
using System;

namespace GuimoSoft.Bus.Kafka.Common
{
    public sealed class KafkaEventsOptions
    {
        public event EventHandler<LogMessage> LogHandler;
        public event EventHandler<Error> ErrorHandler;
        public event EventHandler<Exception> ExceptionHandler;

        internal void OnLog(LogMessage logMessage)
        {
            if (LogHandler is not null)
                LogHandler(this, logMessage);
        }

        internal void OnError(Error error)
        {
            if (ErrorHandler is not null)
                ErrorHandler(this, error);
        }

        internal void OnException(Exception exception)
        {
            if (ExceptionHandler is not null)
                ExceptionHandler(this, exception);
        }

        internal void SetEvents(ConsumerBuilder<string, byte[]> consumerBuilder)
        {
            if (consumerBuilder is null)
                throw new ArgumentNullException(nameof(consumerBuilder));
            consumerBuilder.SetLogHandler((_, logMessage) => OnLog(logMessage));
            consumerBuilder.SetErrorHandler((_, error) => OnError(error));
        }

        internal void SetEvents(ProducerBuilder<string, byte[]> producerBuilder)
        {
            if (producerBuilder is null)
                throw new ArgumentNullException(nameof(producerBuilder));
            producerBuilder.SetLogHandler((_, logMessage) => OnLog(logMessage));
            producerBuilder.SetErrorHandler((_, error) => OnError(error));
        }
    }
}
