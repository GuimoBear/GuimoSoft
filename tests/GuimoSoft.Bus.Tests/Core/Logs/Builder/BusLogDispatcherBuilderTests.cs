using FluentAssertions;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Internal.Middlewares;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Bus.Core.Logs.Builder;
using GuimoSoft.Bus.Core.Logs.Builder.Stages;
using GuimoSoft.Bus.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Logs.Builder
{
    public class BusLogDispatcherBuilderTests
    {
        private static Mock<IServiceProvider> CreateLoggerServiceProvider(Action<BusLogEvent> onLogEvent = null, Action<BusExceptionEvent> onExceptionEvent = null)
        {
            var moqServiceProvider = new Mock<IServiceProvider>();

            moqServiceProvider
                .Setup(x => x.GetService(typeof(EventDispatcherMiddleware<BusLogEvent>)))
                .Returns(new EventDispatcherMiddleware<BusLogEvent>());

            moqServiceProvider
                .Setup(x => x.GetService(typeof(EventDispatcherMiddleware<BusTypedLogEvent<FakeEvent>>)))
                .Returns(new EventDispatcherMiddleware<BusTypedLogEvent<FakeEvent>>());

            moqServiceProvider
                .Setup(x => x.GetService(typeof(EventDispatcherMiddleware<BusExceptionEvent>)))
                .Returns(new EventDispatcherMiddleware<BusExceptionEvent>());

            moqServiceProvider
                .Setup(x => x.GetService(typeof(EventDispatcherMiddleware<BusTypedExceptionEvent<FakeEvent>>)))
                .Returns(new EventDispatcherMiddleware<BusTypedExceptionEvent<FakeEvent>>());

            if (onLogEvent is not null)
            {
                moqServiceProvider
                    .Setup(x => x.GetService(typeof(TestLogEventHandler)))
                    .Returns(new TestLogEventHandler(onLogEvent));
            }

            if (onExceptionEvent is not null)
            {
                moqServiceProvider
                    .Setup(x => x.GetService(typeof(TestExceptionEventHandler)))
                    .Returns(new TestExceptionEventHandler(onExceptionEvent));
            }

            return moqServiceProvider;
        }

        [Fact]
        public void ConstructorShouldCreateBusLogDispatcherBuilder()
        {
            var sut = new BusLogDispatcherBuilder(Mock.Of<IServiceProvider>(), BusName.Kafka);
            Assert.IsType<BusLogDispatcherBuilder>(sut);
        }

        [Fact]
        public void ConstructorShouldThrowIfAnyParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BusLogDispatcherBuilder(null, BusName.Kafka));
        }

        [Fact]
        public void PublishAnLogWithouTEventObjectFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var sc = new ServiceCollection();
                sc.RegisterMediatorFromNewAssemblies(new List<Assembly> { typeof(FakeEvent).Assembly });

                var expectedLogEvent = new BusLogEvent(ServerName.Default)
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Consume,
                    Endpoint = "test",
                    Message = "test event",
                    Level = BusLogLevel.Information
                };
                expectedLogEvent.Data.Add("key-1", "value-1");

                BusLogEvent logEvent = default;

                var moqServiceProvider = CreateLoggerServiceProvider(onLogEvent: @event => logEvent = @event);

                ISwitchStage sut = new BusLogDispatcherBuilder(moqServiceProvider.Object, BusName.Kafka);

                sut
                    .AndSwitch(ServerName.Default).AndFinality(Finality.Consume)
                    .WhileListening().TheEndpoint("test")
                    .Write()
                        .Message("test event")
                        .AndKey("key-1").WithValue("value-1")
                        .With(BusLogLevel.Information)
                    .Publish().AnLog();

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusLogEvent>)), Times.Once);

                logEvent
                    .Should().BeEquivalentTo(expectedLogEvent);

                Utils.ResetarSingletons();
            }
        }

        [Fact]
        public void PublishAnLogWithEventObjectFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var sc = new ServiceCollection();
                sc.RegisterMediatorFromNewAssemblies(new List<Assembly> { typeof(FakeEvent).Assembly });

                var expectedLogEvent = new BusLogEvent(FakeServerName.FakeHost1)
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Produce,
                    Endpoint = "test",
                    Message = "test event",
                    Level = BusLogLevel.Information
                };
                expectedLogEvent.Data.Add("key-1", "value-1");

                var fakeEvent = new FakeEvent("", "");

                var expectedTypedLogEvent = new BusTypedLogEvent<FakeEvent>(expectedLogEvent, fakeEvent);

                var moqServiceProvider = CreateLoggerServiceProvider();

                BusTypedLogEvent<FakeEvent> typedLogEvent = default;

                moqServiceProvider
                    .Setup(x => x.GetService(typeof(TestFakeEventLogHandler)))
                    .Returns(new TestFakeEventLogHandler(@event => typedLogEvent = @event));

                ISwitchStage sut = new BusLogDispatcherBuilder(moqServiceProvider.Object, BusName.Kafka);

                sut
                    .AndSwitch(FakeServerName.FakeHost1).AndFinality(Finality.Produce)
                    .AfterReceived().TheEvent(fakeEvent).FromEndpoint("test")
                    .Write()
                        .Message("test event")
                        .AndKey("key-1").WithValue("value-1")
                        .With(BusLogLevel.Information)
                    .Publish().AnLog();

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusTypedLogEvent<FakeEvent>>)), Times.Once);

                typedLogEvent
                    .Should().BeEquivalentTo(expectedTypedLogEvent);

                Utils.ResetarSingletons();
            }
        }

        [Fact]
        public void PublishAnLogWithEventObjectAndTypedHandlerFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var sc = new ServiceCollection();
                sc.RegisterMediatorFromNewAssemblies(new List<Assembly> { typeof(FakeEvent).Assembly });

                var expectedLogEvent = new BusLogEvent(FakeServerName.FakeHost1)
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Produce,
                    Endpoint = "test",
                    Message = "test event",
                    Level = BusLogLevel.Information
                };
                expectedLogEvent.Data.Add("key-1", "value-1");

                var fakeEvent = new FakeEvent("", "");

                var expectedTypedLogEvent = new BusTypedLogEvent<FakeEvent>(expectedLogEvent, fakeEvent);

                var moqServiceProvider = CreateLoggerServiceProvider();

                BusTypedLogEvent<FakeEvent> typedLogEvent = default;

                moqServiceProvider
                    .Setup(x => x.GetService(typeof(TestFakeEventLogHandler)))
                    .Returns(new TestFakeEventLogHandler(@event => typedLogEvent = @event));

                ISwitchStage sut = new BusLogDispatcherBuilder(moqServiceProvider.Object, BusName.Kafka);

                sut
                    .AndSwitch(FakeServerName.FakeHost1).AndFinality(Finality.Produce)
                    .AfterReceived().TheEvent(fakeEvent).FromEndpoint("test")
                    .Write()
                        .Message("test event")
                        .AndKey("key-1").WithValue("value-1")
                        .With(BusLogLevel.Information)
                    .Publish().AnLog().Wait();

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusTypedLogEvent<FakeEvent>>)), Times.Once);

                typedLogEvent
                    .Should().BeEquivalentTo(expectedTypedLogEvent);

                Utils.ResetarSingletons();
            }
        }

        [Fact]
        public void PublishAnLogWithNullEventObjectFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var sc = new ServiceCollection();
                sc.RegisterMediatorFromNewAssemblies(new List<Assembly> { typeof(FakeEvent).Assembly });

                var expectedLogEvent = new BusLogEvent(ServerName.Default)
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Consume,
                    Endpoint = "test",
                    Message = "test event",
                    Level = BusLogLevel.Information
                };
                expectedLogEvent.Data.Add("key-1", "value-1");

                BusLogEvent logEvent = default;

                var moqServiceProvider = CreateLoggerServiceProvider(onLogEvent: @event => logEvent = @event);

                ISwitchStage sut = new BusLogDispatcherBuilder(moqServiceProvider.Object, BusName.Kafka);

                sut
                    .AndSwitch(ServerName.Default).AndFinality(Finality.Consume)
                    .AfterReceived().TheEvent(null).FromEndpoint("test")
                    .Write()
                        .Message("test event")
                        .AndKey("key-1").WithValue("value-1")
                        .With(BusLogLevel.Information)
                    .Publish().AnLog();

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusLogEvent>)), Times.Once);

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusTypedLogEvent<FakeEvent>>)), Times.Never);

                logEvent
                    .Should().BeEquivalentTo(expectedLogEvent);

                Utils.ResetarSingletons();
            }
        }

        [Fact]
        public void PublishAnExceptionWithouTEventObjectFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var sc = new ServiceCollection();
                sc.RegisterMediatorFromNewAssemblies(new List<Assembly> { typeof(FakeEvent).Assembly });

                var expectedExceptionEvent = new BusExceptionEvent(ServerName.Default, new Exception(""))
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Produce,
                    Endpoint = "test",
                    Message = "test event",
                    Level = BusLogLevel.Error
                };
                expectedExceptionEvent.Data.Add("key-1", "value-1");

                BusExceptionEvent exceptionEvent = default;

                var moqServiceProvider = CreateLoggerServiceProvider(onExceptionEvent: @event => exceptionEvent = @event);

                var sut = new BusLogDispatcherBuilder(moqServiceProvider.Object, BusName.Kafka)
                    .AndSwitch(ServerName.Default).AndFinality(Finality.Produce)
                    .WhileListening().TheEndpoint("test")
                    .Write()
                        .Message("test event")
                        .AndKey("key-1").WithValue("value-1")
                        .With(BusLogLevel.Error)
                    .Publish();

                Assert.Throws<ArgumentNullException>(() => sut.AnException(null).ConfigureAwait(false).GetAwaiter().GetResult());

                sut.AnException(new Exception(""));

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusExceptionEvent>)), Times.Once);

                exceptionEvent
                    .Should().BeEquivalentTo(expectedExceptionEvent);

                Utils.ResetarSingletons();
            }
        }

        [Fact]
        public void PublishAnExceptionWithEventObjectFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var sc = new ServiceCollection();
                sc.RegisterMediatorFromNewAssemblies(new List<Assembly> { typeof(FakeEvent).Assembly });

                var expectedExceptionEvent = new BusExceptionEvent(FakeServerName.FakeHost1, new Exception(""))
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Consume,
                    Endpoint = "test",
                    Message = "test event",
                    Level = BusLogLevel.Error
                };
                expectedExceptionEvent.Data.Add("key-1", "value-1");

                var fakeEvent = new FakeEvent("", "");

                var expectedTypedExceptionEvent = new BusTypedExceptionEvent<FakeEvent>(expectedExceptionEvent, fakeEvent);

                var moqServiceProvider = CreateLoggerServiceProvider();

                BusTypedExceptionEvent<FakeEvent> typedExceptionEvent = default;

                moqServiceProvider
                    .Setup(x => x.GetService(typeof(TestFakeEventExceptionHandler)))
                    .Returns(new TestFakeEventExceptionHandler(@event => typedExceptionEvent = @event));

                ISwitchStage sut = new BusLogDispatcherBuilder(moqServiceProvider.Object, BusName.Kafka);

                sut
                    .AndSwitch(FakeServerName.FakeHost1).AndFinality(Finality.Consume)
                    .AfterReceived().TheEvent(fakeEvent).FromEndpoint("test")
                    .Write()
                        .Message("test event")
                        .AndKey("key-1").WithValue("value-1")
                        .With(BusLogLevel.Error)
                    .Publish().AnException(new Exception(""));

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusTypedExceptionEvent<FakeEvent>>)), Times.Once);

                typedExceptionEvent
                    .Should().BeEquivalentTo(expectedTypedExceptionEvent);

                Utils.ResetarSingletons();
            }
        }

        [Fact]
        public void PublishAnExceptionWithEventObjectAndTypedHandlerFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var sc = new ServiceCollection();
                sc.RegisterMediatorFromNewAssemblies(new List<Assembly> { typeof(FakeEvent).Assembly });

                var expectedExceptionEvent = new BusExceptionEvent(FakeServerName.FakeHost1, new Exception(""))
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Consume,
                    Endpoint = "test",
                    Message = "test event",
                    Level = BusLogLevel.Error
                };
                expectedExceptionEvent.Data.Add("key-1", "value-1");

                var fakeEvent = new FakeEvent("", "");

                var expectedTypedExceptionEvent = new BusTypedExceptionEvent<FakeEvent>(expectedExceptionEvent, fakeEvent);

                var moqServiceProvider = CreateLoggerServiceProvider();

                BusTypedExceptionEvent<FakeEvent> typedExceptionEvent = default;

                moqServiceProvider
                    .Setup(x => x.GetService(typeof(TestFakeEventExceptionHandler)))
                    .Returns(new TestFakeEventExceptionHandler(@event => typedExceptionEvent = @event));

                ISwitchStage sut = new BusLogDispatcherBuilder(moqServiceProvider.Object, BusName.Kafka);

                sut
                    .AndSwitch(FakeServerName.FakeHost1).AndFinality(Finality.Consume)
                    .AfterReceived().TheEvent(fakeEvent).FromEndpoint("test")
                    .Write()
                        .Message("test event")
                        .AndKey("key-1").WithValue("value-1")
                        .With(BusLogLevel.Error)
                    .Publish().AnException(new Exception(""));

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusExceptionEvent>)), Times.Never);

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusTypedExceptionEvent<FakeEvent>>)), Times.Once);

                typedExceptionEvent
                    .Should().BeEquivalentTo(expectedTypedExceptionEvent);

                Utils.ResetarSingletons();
            }
        }

        [Fact]
        public void PublishAnExceptionWithNullEventObjectFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var sc = new ServiceCollection();
                sc.RegisterMediatorFromNewAssemblies(new List<Assembly> { typeof(FakeEvent).Assembly });

                var expectedExceptionEvent = new BusExceptionEvent(ServerName.Default, new Exception(""))
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Produce,
                    Endpoint = "test",
                    Message = "test event",
                    Level = BusLogLevel.Error
                };
                expectedExceptionEvent.Data.Add("key-1", "value-1");

                BusExceptionEvent exceptionEvent = default;

                var moqServiceProvider = CreateLoggerServiceProvider(onExceptionEvent: @event => exceptionEvent = @event);

                ISwitchStage sut = new BusLogDispatcherBuilder(moqServiceProvider.Object, BusName.Kafka);

                sut
                    .AndSwitch(ServerName.Default).AndFinality(Finality.Produce)
                    .AfterReceived().TheEvent(null).FromEndpoint("test")
                    .Write()
                        .Message("test event")
                        .AndKey("key-1").WithValue("value-1")
                        .With(BusLogLevel.Error)
                    .Publish().AnException(new Exception(""));


                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusExceptionEvent>)), Times.Once);

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusTypedExceptionEvent<FakeEvent>>)), Times.Never);

                exceptionEvent
                    .Should().BeEquivalentTo(expectedExceptionEvent);

                Utils.ResetarSingletons();
            }
        }

        private class TestLogEventHandler : IEventHandler<BusLogEvent>
        {
            private readonly Action<BusLogEvent> _onInvoke;

            public TestLogEventHandler(Action<BusLogEvent> onInvoke)
                => _onInvoke = onInvoke;

            public Task Handle(BusLogEvent  @event, CancellationToken cancellationToken)
            {
                _onInvoke(@event);
                return Task.CompletedTask;
            }
        }

        private class TestFakeEventLogHandler : IEventHandler<BusTypedLogEvent<FakeEvent>>
        {
            private readonly Action<BusTypedLogEvent<FakeEvent>> _onInvoke;

            public TestFakeEventLogHandler(Action<BusTypedLogEvent<FakeEvent>> onInvoke)
                => _onInvoke = onInvoke;

            public Task Handle(BusTypedLogEvent<FakeEvent> @event, CancellationToken cancellationToken)
            {
                _onInvoke(@event);
                return Task.CompletedTask;
            }
        }

        private class TestExceptionEventHandler : IEventHandler<BusExceptionEvent>
        {
            private readonly Action<BusExceptionEvent> _onInvoke;

            public TestExceptionEventHandler(Action<BusExceptionEvent> onInvoke)
                => _onInvoke = onInvoke;

            public Task Handle(BusExceptionEvent @event, CancellationToken cancellationToken)
            {
                _onInvoke(@event);
                return Task.CompletedTask;
            }
        }

        private class TestFakeEventExceptionHandler : IEventHandler<BusTypedExceptionEvent<FakeEvent>>
        {
            private readonly Action<BusTypedExceptionEvent<FakeEvent>> _onInvoke;

            public TestFakeEventExceptionHandler(Action<BusTypedExceptionEvent<FakeEvent>> onInvoke)
                => _onInvoke = onInvoke;

            public Task Handle(BusTypedExceptionEvent<FakeEvent> @event, CancellationToken cancellationToken)
            {
                _onInvoke(@event);
                return Task.CompletedTask;
            }
        }
    }
}
