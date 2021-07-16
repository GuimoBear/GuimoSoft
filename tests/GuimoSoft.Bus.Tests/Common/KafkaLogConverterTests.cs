using Confluent.Kafka;
using FluentAssertions;
using GuimoSoft.Bus.Kafka.Common;
using Microsoft.Extensions.Logging;
using Xunit;

namespace GuimoSoft.Bus.Tests.Common
{
    public class KafkaLogConverterTests
    {
        [Fact]
        public void CastLogMessageFacts()
        {
            KafkaLogConverter.Cast((LogMessage)null)
                .Should().BeNull();

            var logMessage = new LogMessage("test", SyslogLevel.Debug, "test", "test");

            var expected = new Bus.Core.Logs.LogMessage(logMessage.Message, (LogLevel)logMessage.LevelAs(LogLevelType.MicrosoftExtensionsLogging));
            expected.Data.Add(nameof(logMessage.Name), logMessage.Name);
            expected.Data.Add(nameof(logMessage.Facility), logMessage.Facility);

            KafkaLogConverter.Cast(logMessage)
                .Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void CastErrorFacts()
        {
            KafkaLogConverter.Cast((Error)null)
                .Should().BeNull();

            var errorMessage = new Error(ErrorCode.BrokerNotAvailable, "test", true);

            var expected = new Bus.Core.Logs.ExceptionMessage(errorMessage.Reason, LogLevel.Error) { Exception = new KafkaException(errorMessage) };
            expected.Data.Add(nameof(errorMessage.Code), errorMessage.Code);
            expected.Data.Add(nameof(errorMessage.IsBrokerError), errorMessage.IsBrokerError);
            expected.Data.Add(nameof(errorMessage.IsError), errorMessage.IsError);
            expected.Data.Add(nameof(errorMessage.IsFatal), errorMessage.IsFatal);
            expected.Data.Add(nameof(errorMessage.IsLocalError), errorMessage.IsLocalError);

            KafkaLogConverter.Cast(errorMessage)
                .Should().BeEquivalentTo(expected);
        }
    }
}
