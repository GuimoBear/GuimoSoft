using Confluent.Kafka;
using FluentAssertions;
using GuimoSoft.Bus.Kafka.Common;
using System;
using System.Threading;
using Xunit;

namespace GuimoSoft.Bus.Tests.Common
{
    public class KafkaEventsOptionsTests
    {
        [Fact]
        public void OnLogFacts()
        {
            var sut = new KafkaEventsOptions();

            sut.OnLog(new LogMessage("", SyslogLevel.Alert, "", ""));

            var logHandlerCalled = new AsyncLocal<bool>();
            logHandlerCalled.Value = false;
            sut.LogHandler += (_, _) => logHandlerCalled.Value = true;

            sut.OnLog(new LogMessage("", SyslogLevel.Alert, "", ""));

            logHandlerCalled.Value
                .Should().BeTrue();
        }

        [Fact]
        public void OnErrorFacts()
        {
            var sut = new KafkaEventsOptions();

            sut.OnError(new Error(ErrorCode.BrokerNotAvailable));

            var errorHandlerCalled = new AsyncLocal<bool>();
            errorHandlerCalled.Value = false;
            sut.ErrorHandler += (_, _) => errorHandlerCalled.Value = true;

            sut.OnError(new Error(ErrorCode.BrokerNotAvailable));

            errorHandlerCalled.Value
                .Should().BeTrue();
        }

        [Fact]
        public void OnExceptionFacts()
        {
            var sut = new KafkaEventsOptions();

            sut.OnException(new Exception());

            var exceptionHandlerCalled = new AsyncLocal<bool>();
            exceptionHandlerCalled.Value = false;
            sut.ExceptionHandler += (_, _) => exceptionHandlerCalled.Value = true;

            sut.OnException(new Exception());

            exceptionHandlerCalled.Value
                .Should().BeTrue();
        }

        [Fact]
        public void SetEventsOnConsumerBuilderFacts()
        {
            var sut = new KafkaEventsOptions();

            Assert.Throws<ArgumentNullException>(() => sut.SetEvents(default(ConsumerBuilder<string, byte[]>)));

            var consumerBuilder = new ConsumerBuilder<string, byte[]>(new ConsumerConfig());

            sut.SetEvents(consumerBuilder);
        }

        [Fact]
        public void SetEventsOnProducerBuilderFacts()
        {
            var sut = new KafkaEventsOptions();

            Assert.Throws<ArgumentNullException>(() => sut.SetEvents(default(ProducerBuilder<string, byte[]>)));

            var consumerBuilder = new ProducerBuilder<string, byte[]>(new ProducerConfig());

            sut.SetEvents(consumerBuilder);
        }
    }
}
