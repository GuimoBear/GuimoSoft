using DeepEqual.Syntax;
using GuimoSoft.Bus.Abstractions;
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
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Logs.Builder
{
    public class BusLogDispatcherBuilderTests
    {
        private static Mock<IServiceProvider> CreateLoggerServiceProvider()
        {
            var moqServiceProvider = new Mock<IServiceProvider>();

            moqServiceProvider
                .Setup(x => x.GetService(typeof(EventDispatcherMiddleware<BusLogEvent>)))
                .Returns(new EventDispatcherMiddleware<BusLogEvent>());

            moqServiceProvider
                .Setup(x => x.GetService(typeof(EventDispatcherMiddleware<BusExceptionEvent>)))
                .Returns(new EventDispatcherMiddleware<BusExceptionEvent>());

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
                var expectedLogEvent = new BusLogEvent(ServerName.Default)
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Consume,
                    Endpoint = "test",
                    Message = "test event",
                    Level = BusLogLevel.Information
                };
                expectedLogEvent.Data.Add("key-1", "value-1");

                var moqServiceProvider = CreateLoggerServiceProvider();

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

                moqServiceProvider
                    .Verify(x => x.GetService(It.Is<Type>(type => !type.Equals(typeof(EventDispatcherMiddleware<BusLogEvent>)))), Times.Never);
            }
        }

        [Fact]
        public void PublishAnLogWithEventObjectFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
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
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusLogEvent>)), Times.Once);

                moqServiceProvider
                    .Verify(x => x.GetService(It.Is<Type>(type => !type.Equals(typeof(EventDispatcherMiddleware<BusLogEvent>)))), Times.Never);
            }
        }

        [Fact]
        public void PublishAnLogWithEventObjectAndTypedHandlerFacts()
        {
            lock (Utils.Lock)
            {
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
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusLogEvent>)), Times.Never);

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusTypedLogEvent<FakeEvent>>)), Times.Once);

                Utils.ResetarSingletons();
            }
        }

        [Fact]
        public void PublishAnLogWithNullEventObjectFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var expectedLogEvent = new BusLogEvent(ServerName.Default)
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Consume,
                    Endpoint = "test",
                    Message = "test event",
                    Level = BusLogLevel.Information
                };
                expectedLogEvent.Data.Add("key-1", "value-1");

                var expectedTypedLogEvent = new BusTypedLogEvent<FakeEvent>(expectedLogEvent, null);

                var moqServiceProvider = CreateLoggerServiceProvider();

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
            }
        }

        [Fact]
        public void PublishAnExceptionWithouTEventObjectFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var expectedExceptionEvent = new BusExceptionEvent(ServerName.Default, new Exception(""))
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Produce,
                    Endpoint = "test",
                    Message = "test event",
                    Level = BusLogLevel.Error
                };
                expectedExceptionEvent.Data.Add("key-1", "value-1");

                var moqServiceProvider = CreateLoggerServiceProvider();

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

                moqServiceProvider
                    .Verify(x => x.GetService(It.Is<Type>(type => !type.Equals(typeof(EventDispatcherMiddleware<BusExceptionEvent>)))), Times.Never);
            }
        }

        [Fact]
        public void PublishAnExceptionWithEventObjectFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
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
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusExceptionEvent>)), Times.Once);

                moqServiceProvider
                    .Verify(x => x.GetService(It.Is<Type>(type => !type.Equals(typeof(EventDispatcherMiddleware<BusExceptionEvent>)))), Times.Never);
            }
        }

        [Fact]
        public void PublishAnExceptionWithEventObjectAndTypedHandlerFacts()
        {
            lock (Utils.Lock)
            {
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

                Utils.ResetarSingletons();
            }
        }

        [Fact]
        public void PublishAnExceptionWithNullEventObjectFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var expectedExceptionEvent = new BusExceptionEvent(ServerName.Default, new Exception(""))
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Produce,
                    Endpoint = "test",
                    Message = "test event",
                    Level = BusLogLevel.Error
                };
                expectedExceptionEvent.Data.Add("key-1", "value-1");

                var expectedTypedExceptionEvent = new BusTypedExceptionEvent<FakeEvent>(expectedExceptionEvent, null);

                var moqServiceProvider = CreateLoggerServiceProvider();

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
            }
        }

        private static bool IsEqual(BusLogEvent expected, object actual)
            => expected.IsDeepEqual(actual);

        private static bool IsEqual(BusTypedLogEvent<FakeEvent> expected, object actual)
            => expected.IsDeepEqual(actual);

        private static bool IsEqual(BusTypedExceptionEvent<FakeEvent> expected, object actual)
            => expected.IsDeepEqual(actual);

        private static bool IsEqual(BusExceptionEvent expected, object actual)
            => expected.IsDeepEqual(actual);
    }
}
