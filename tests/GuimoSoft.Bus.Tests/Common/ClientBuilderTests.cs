using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Core.Internal.Middlewares;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Bus.Core.Logs.Builder;
using GuimoSoft.Bus.Tests.Fakes;
using Moq;
using System;
using Xunit;

namespace GuimoSoft.Bus.Tests.Common
{
    public class ClientBuilderTests
    {
        public Type[] BusLogMessage { get; private set; }

        [Fact]
        public void LogEventFacts()
        {
            var (moqLogger, moqServiceProvider) = CreateLoggerMock(BusName.Kafka);

            var sut = new FakeClientBuilder(moqLogger.Object);

            sut.WriteLog();

            moqServiceProvider
                .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusLogEvent>)), Times.Once);
        }

        [Fact]
        public void LogExceptionFacts()
        {
            var (moqLogger, moqServiceProvider) = CreateLoggerMock(BusName.Kafka);

            var sut = new FakeClientBuilder(moqLogger.Object);

            sut.WriteException();

            moqServiceProvider
                .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusExceptionEvent>)), Times.Once);
        }

        private static (Mock<IBusLogDispatcher>, Mock<IServiceProvider>) CreateLoggerMock(BusName bus)
        {
            var moqServiceProvider = new Mock<IServiceProvider>();

            moqServiceProvider
                .Setup(x => x.GetService(typeof(EventDispatcherMiddleware<BusLogEvent>)))
                .Returns(new EventDispatcherMiddleware<BusLogEvent>());

            moqServiceProvider
                .Setup(x => x.GetService(typeof(EventDispatcherMiddleware<BusExceptionEvent>)))
                .Returns(new EventDispatcherMiddleware<BusExceptionEvent>());

            var moqLogger = new Mock<IBusLogDispatcher>();

            moqLogger
                .Setup(x => x.FromBus(bus)).Returns(() => new BusLogDispatcherBuilder(moqServiceProvider.Object, bus));

            return (moqLogger, moqServiceProvider);
        }
    }
}
