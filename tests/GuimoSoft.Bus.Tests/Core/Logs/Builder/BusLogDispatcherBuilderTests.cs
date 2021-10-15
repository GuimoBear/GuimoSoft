using DeepEqual.Syntax;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Bus.Core.Logs.Builder;
using GuimoSoft.Bus.Core.Logs.Builder.Stages;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Logs.Builder
{
    public class BusLogDispatcherBuilderTests
    {
        [Fact]
        public void ConstructorShouldCreateBusLogDispatcherBuilder()
        {
            var sut = new BusLogDispatcherBuilder(Mock.Of<IMediator>(), BusName.Kafka);
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

                var moqMediator = new Mock<IMediator>();

                ISwitchStage sut = new BusLogDispatcherBuilder(moqMediator.Object, BusName.Kafka);

                sut
                    .AndSwitch(ServerName.Default).AndFinality(Finality.Consume)
                    .WhileListening().TheEndpoint("test")
                    .Write()
                        .Message("test event")
                        .AndKey("key-1").FromValue("value-1")
                        .With(BusLogLevel.Information)
                    .Publish().AnLog();

                moqMediator
                    .Verify(x => x.Publish(It.Is<BusLogEvent>(actual => IsEqual(expectedLogEvent, actual)), It.IsAny<CancellationToken>()), Times.Once);
                moqMediator
                    .Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
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

                var moqMediator = new Mock<IMediator>();

                ISwitchStage sut = new BusLogDispatcherBuilder(moqMediator.Object, BusName.Kafka);

                sut
                    .AndSwitch(FakeServerName.FakeHost1).AndFinality(Finality.Produce)
                    .AfterReceived().TheEvent(fakeEvent).FromEndpoint("test")
                    .Write()
                        .Message("test event")
                        .AndKey("key-1").FromValue("value-1")
                        .With(BusLogLevel.Information)
                    .Publish().AnLog();

                moqMediator
                    .Verify(x => x.Publish(It.Is<BusLogEvent>(actual => IsEqual(expectedLogEvent, actual)), It.IsAny<CancellationToken>()), Times.Once);
                moqMediator
                    .Verify(x => x.Publish(It.Is<object>((obj, _) => IsEqual(expectedTypedLogEvent, obj)), It.IsAny<CancellationToken>()), Times.Never);
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

                var moqMediator = new Mock<IMediator>();

                ISwitchStage sut = new BusLogDispatcherBuilder(moqMediator.Object, BusName.Kafka);

                sut
                    .AndSwitch(FakeServerName.FakeHost1).AndFinality(Finality.Produce)
                    .AfterReceived().TheEvent(fakeEvent).FromEndpoint("test")
                    .Write()
                        .Message("test event")
                        .AndKey("key-1").FromValue("value-1")
                        .With(BusLogLevel.Information)
                    .Publish().AnLog();

                moqMediator
                    .Verify(x => x.Publish(It.Is<BusLogEvent>(actual => IsEqual(expectedLogEvent, actual)), It.IsAny<CancellationToken>()), Times.Never);
                moqMediator
                    .Verify(x => x.Publish(It.Is<object>((obj, _) => IsEqual(expectedTypedLogEvent, obj)), It.IsAny<CancellationToken>()), Times.Once);

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

                var moqMediator = new Mock<IMediator>();

                ISwitchStage sut = new BusLogDispatcherBuilder(moqMediator.Object, BusName.Kafka);

                sut
                    .AndSwitch(ServerName.Default).AndFinality(Finality.Consume)
                    .AfterReceived().TheEvent(null).FromEndpoint("test")
                    .Write()
                        .Message("test event")
                        .AndKey("key-1").FromValue("value-1")
                        .With(BusLogLevel.Information)
                    .Publish().AnLog();

                moqMediator
                    .Verify(x => x.Publish(It.Is<BusLogEvent>(actual => IsEqual(expectedLogEvent, actual)), It.IsAny<CancellationToken>()), Times.Once);
                moqMediator
                    .Verify(x => x.Publish(It.Is<object>((obj, _) => IsEqual(expectedTypedLogEvent, obj)), It.IsAny<CancellationToken>()), Times.Never);
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

                var moqMediator = new Mock<IMediator>();

                var sut = new BusLogDispatcherBuilder(moqMediator.Object, BusName.Kafka)
                    .AndSwitch(ServerName.Default).AndFinality(Finality.Produce)
                    .WhileListening().TheEndpoint("test")
                    .Write()
                        .Message("test event")
                        .AndKey("key-1").FromValue("value-1")
                        .With(BusLogLevel.Error)
                    .Publish();

                Assert.Throws<ArgumentNullException>(() => sut.AnException(null).ConfigureAwait(false).GetAwaiter().GetResult());

                sut.AnException(new Exception(""));

                moqMediator
                    .Verify(x => x.Publish(It.Is<BusExceptionEvent>(actual => IsEqual(expectedExceptionEvent, actual)), It.IsAny<CancellationToken>()), Times.Once);
                moqMediator
                    .Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
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

                var moqMediator = new Mock<IMediator>();

                ISwitchStage sut = new BusLogDispatcherBuilder(moqMediator.Object, BusName.Kafka);

                sut
                    .AndSwitch(FakeServerName.FakeHost1).AndFinality(Finality.Consume)
                    .AfterReceived().TheEvent(fakeEvent).FromEndpoint("test")
                    .Write()
                        .Message("test event")
                        .AndKey("key-1").FromValue("value-1")
                        .With(BusLogLevel.Error)
                    .Publish().AnException(new Exception(""));

                moqMediator
                    .Verify(x => x.Publish(It.Is<BusExceptionEvent>(actual => IsEqual(expectedExceptionEvent, actual)), It.IsAny<CancellationToken>()), Times.Once);
                moqMediator
                    .Verify(x => x.Publish(It.Is<object>((obj, _) => IsEqual(expectedTypedExceptionEvent, obj)), It.IsAny<CancellationToken>()), Times.Never);
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

                var moqMediator = new Mock<IMediator>();

                ISwitchStage sut = new BusLogDispatcherBuilder(moqMediator.Object, BusName.Kafka);

                sut
                    .AndSwitch(FakeServerName.FakeHost1).AndFinality(Finality.Consume)
                    .AfterReceived().TheEvent(fakeEvent).FromEndpoint("test")
                    .Write()
                        .Message("test event")
                        .AndKey("key-1").FromValue("value-1")
                        .With(BusLogLevel.Error)
                    .Publish().AnException(new Exception(""));

                moqMediator
                    .Verify(x => x.Publish(It.Is<BusExceptionEvent>(actual => IsEqual(expectedExceptionEvent, actual)), It.IsAny<CancellationToken>()), Times.Never);
                moqMediator
                    .Verify(x => x.Publish(It.Is<object>((obj, _) => IsEqual(expectedTypedExceptionEvent, obj)), It.IsAny<CancellationToken>()), Times.Once);
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

                var moqMediator = new Mock<IMediator>();

                ISwitchStage sut = new BusLogDispatcherBuilder(moqMediator.Object, BusName.Kafka);

                sut
                    .AndSwitch(ServerName.Default).AndFinality(Finality.Produce)
                    .AfterReceived().TheEvent(null).FromEndpoint("test")
                    .Write()
                        .Message("test event")
                        .AndKey("key-1").FromValue("value-1")
                        .With(BusLogLevel.Error)
                    .Publish().AnException(new Exception(""));

                moqMediator
                    .Verify(x => x.Publish(It.Is<BusExceptionEvent>(actual => IsEqual(expectedExceptionEvent, actual)), It.IsAny<CancellationToken>()), Times.Once);
                moqMediator
                    .Verify(x => x.Publish(It.Is<object>((obj, _) => IsEqual(expectedTypedExceptionEvent, obj)), It.IsAny<CancellationToken>()), Times.Never);
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
