using GuimoSoft.Bus.Core.Logs;
using Microsoft.Extensions.Logging;
using Error = Confluent.Kafka.Error;
using KafkaLogMessage = Confluent.Kafka.LogMessage;
using LogLevelType = Confluent.Kafka.LogLevelType;

namespace GuimoSoft.Bus.Kafka.Common
{
    internal static class KafkaLogConverter
    {
        public static LogMessage Cast(KafkaLogMessage kafkaLogMessage)
        {
            if (kafkaLogMessage is null)
                return null;

            var logMessage = new LogMessage(kafkaLogMessage.Message, (LogLevel)kafkaLogMessage.LevelAs(LogLevelType.MicrosoftExtensionsLogging));
            logMessage.Data.Add(nameof(kafkaLogMessage.Name), kafkaLogMessage.Name);
            logMessage.Data.Add(nameof(kafkaLogMessage.Facility), kafkaLogMessage.Facility);

            return logMessage;
        }
        public static ExceptionMessage Cast(Error kafkaError)
        {
            if (kafkaError is null)
                return null;
            var exceptionMessage = new ExceptionMessage(kafkaError.Reason, LogLevel.Error)
            {
                Exception = new Confluent.Kafka.KafkaException(kafkaError)
            };
            exceptionMessage.Data.Add(nameof(kafkaError.Code), kafkaError.Code);
            exceptionMessage.Data.Add(nameof(kafkaError.IsBrokerError), kafkaError.IsBrokerError);
            exceptionMessage.Data.Add(nameof(kafkaError.IsError), kafkaError.IsError);
            exceptionMessage.Data.Add(nameof(kafkaError.IsFatal), kafkaError.IsFatal);
            exceptionMessage.Data.Add(nameof(kafkaError.IsLocalError), kafkaError.IsLocalError);

            return exceptionMessage;
        }
    }
}
