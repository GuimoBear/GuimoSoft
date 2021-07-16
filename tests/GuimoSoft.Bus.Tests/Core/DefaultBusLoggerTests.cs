using GuimoSoft.Bus.Core.Logs;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core
{
    public class DefaultBusLoggerTests
    {
        [Fact]
        public void ConstructorShouldThrowArgumentNullExceptionIfLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DefaultBusLogger(null));
        }

        [Fact]
        public async Task LogAsyncFacts()
        {
            var moqLogger = CreateLoggerMock<DefaultBusLogger>();

            var sut = new DefaultBusLogger(moqLogger.Object);

            await sut.LogAsync(null);

            moqLogger
                .Verify(x =>
                    x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.IsAny<Exception>(), It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Never);

            await sut.LogAsync(LogMessage.Trace("Trace log"));
            await sut.LogAsync(LogMessage.Debug("Debug log"));
            await sut.LogAsync(LogMessage.Information("Information log"));
            await sut.LogAsync(LogMessage.Warning("Warning log"));
            await sut.LogAsync(LogMessage.Error("Error log"));
            await sut.LogAsync(LogMessage.Critical("Critical log"));
            await sut.LogAsync(LogMessage.None("None log"));

            moqLogger
                .Verify(x =>
                    x.Log(LogLevel.Trace, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.IsAny<Exception>(), It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Once);
            moqLogger
                .Verify(x =>
                    x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.IsAny<Exception>(), It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Once);
            moqLogger
                .Verify(x =>
                    x.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.IsAny<Exception>(), It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Once);
            moqLogger
                .Verify(x =>
                    x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.IsAny<Exception>(), It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Once);
            moqLogger
                .Verify(x =>
                    x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.IsAny<Exception>(), It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Once);
            moqLogger
                .Verify(x =>
                    x.Log(LogLevel.Critical, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.IsAny<Exception>(), It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Once);
            moqLogger
                .Verify(x =>
                    x.Log(LogLevel.None, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.IsAny<Exception>(), It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Once);
        }

        [Fact]
        public async Task ExceptionAsyncFacts()
        {
            var moqLogger = CreateLoggerMock<DefaultBusLogger>();

            var sut = new DefaultBusLogger(moqLogger.Object);

            await sut.ExceptionAsync(null);

            moqLogger
                .Verify(x =>
                    x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.IsAny<Exception>(), It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Never);

            await sut.ExceptionAsync(new ExceptionMessage("Trace log", LogLevel.Trace) { Exception = new ArgumentNullException() });
            await sut.ExceptionAsync(new ExceptionMessage("Debug log", LogLevel.Debug) { Exception = new ArgumentNullException() });
            await sut.ExceptionAsync(new ExceptionMessage("Information log", LogLevel.Information) { Exception = new ArgumentNullException() });
            await sut.ExceptionAsync(new ExceptionMessage("Warning log", LogLevel.Warning) { Exception = new ArgumentNullException() });
            await sut.ExceptionAsync(new ExceptionMessage("Error log", LogLevel.Error) { Exception = new ArgumentNullException() });
            await sut.ExceptionAsync(new ExceptionMessage("Critical log", LogLevel.Critical) { Exception = new ArgumentNullException() });
            await sut.ExceptionAsync(new ExceptionMessage("None log", LogLevel.None) { Exception = new ArgumentNullException() });

            moqLogger
                .Verify(x =>
                    x.Log(LogLevel.Trace, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.Is<Exception>((obj, _) => obj.GetType().Equals(typeof(ArgumentNullException))), It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Once);
            moqLogger
                .Verify(x =>
                    x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.Is<Exception>((obj, _) => obj.GetType().Equals(typeof(ArgumentNullException))), It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Once);
            moqLogger
                .Verify(x =>
                    x.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.Is<Exception>((obj, _) => obj.GetType().Equals(typeof(ArgumentNullException))), It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Once);
            moqLogger
                .Verify(x =>
                    x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.Is<Exception>((obj, _) => obj.GetType().Equals(typeof(ArgumentNullException))), It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Once);
            moqLogger
                .Verify(x =>
                    x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.Is<Exception>((obj, _) => obj.GetType().Equals(typeof(ArgumentNullException))), It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Once);
            moqLogger
                .Verify(x =>
                    x.Log(LogLevel.Critical, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.Is<Exception>((obj, _) => obj.GetType().Equals(typeof(ArgumentNullException))), It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Once);
            moqLogger
                .Verify(x =>
                    x.Log(LogLevel.None, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.Is<Exception>((obj, _) => obj.GetType().Equals(typeof(ArgumentNullException))), It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Once);
        }

        private static Mock<ILogger<TCategory>> CreateLoggerMock<TCategory>()
        {
            var moqLogger = new Mock<ILogger<TCategory>>();
            moqLogger
                .Setup(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ))
                .Verifiable();

            return moqLogger;
        }
    }
}
