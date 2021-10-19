using Confluent.Kafka;
using System;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Core.Logs;

namespace GuimoSoft.Bus.Kafka.Common
{
    internal abstract class ClientBuilder
    {
        private readonly IBusLogDispatcher _logger;

        protected ClientBuilder(IBusLogDispatcher logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected void LogEvent(Enum @switch, Finality finality, LogMessage logEvent)
        {
            _logger
                .FromBus(BusName.Kafka).AndSwitch(@switch).AndFinality(finality)
                .Write()
                    .Message(logEvent.Message)
                    .AndKey(nameof(logEvent.Name)).WithValue(logEvent.Name)
                    .AndKey(nameof(logEvent.Facility)).WithValue(logEvent.Facility)
                    .With((BusLogLevel)logEvent.LevelAs(LogLevelType.MicrosoftExtensionsLogging))
                .Publish().AnLog()
                .ConfigureAwait(false);
        }

        protected void LogException(Enum @switch, Finality finality, Error error)
        {
            _logger
                .FromBus(BusName.Kafka).AndSwitch(@switch).AndFinality(finality)
                .Write()
                    .Message(error.Reason)
                    .AndKey(nameof(error.Code)).WithValue(error.Code)
                    .AndKey(nameof(error.IsBrokerError)).WithValue(error.IsBrokerError)
                    .AndKey(nameof(error.IsError)).WithValue(error.IsError)
                    .AndKey(nameof(error.IsFatal)).WithValue(error.IsFatal)
                    .AndKey(nameof(error.IsLocalError)).WithValue(error.IsLocalError)
                    .With(BusLogLevel.Error)
                .Publish().AnException(new KafkaException(error))
                .ConfigureAwait(false);
        }
    }
}
