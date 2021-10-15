using Confluent.Kafka;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Bus.Core.Logs.Builder;
using GuimoSoft.Bus.Kafka.Consumer;
using GuimoSoft.Bus.Tests.Fakes;
using GuimoSoft.Core.Serialization;
using GuimoSoft.Core.Serialization.Interfaces;
using Xunit;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class KafkaTopicEventConsumerTests
    {
        public static readonly IEnumerable<object[]> ConstructorData
            = new List<object[]>
            {
                new object[] { null, null, null, null, null, null },
                new object[] { Mock.Of<IKafkaConsumerBuilder>(), null, null, null, null, null },
                new object[] { Mock.Of<IKafkaConsumerBuilder>(), Mock.Of<IServiceProvider>(), null, null, null, null },
                new object[] { Mock.Of<IKafkaConsumerBuilder>(), Mock.Of<IServiceProvider>(), Mock.Of<IEventTypeCache>(), null, null, null },
                new object[] { Mock.Of<IKafkaConsumerBuilder>(), Mock.Of<IServiceProvider>(), Mock.Of<IEventTypeCache>(), Mock.Of<IEventMiddlewareExecutorProvider>(), null, null },
                new object[] { Mock.Of<IKafkaConsumerBuilder>(), Mock.Of<IServiceProvider>(), Mock.Of<IEventTypeCache>(), Mock.Of<IEventMiddlewareExecutorProvider>(), Mock.Of<IBusSerializerManager>(), null }
            };

        private static (Mock<IBusLogDispatcher>, Mock<IMediator>) CreateLoggerMock<TEvent>(BusName bus)
            where TEvent : IEvent
        {
            var moqMediator = new Mock<IMediator>();

            var logBuilder = new BusLogDispatcherBuilder(moqMediator.Object, bus);

            var moqLogger = new Mock<IBusLogDispatcher>();

            moqLogger
                .Setup(x => x.FromBus(bus)).Returns(logBuilder);

            return (moqLogger, moqMediator);
        }

        private static Mock<IEventTypeCache> CreateeventTypeTopicCache(string topicName)
        {
            var mockeventTypeCache = new Mock<IEventTypeCache>();

            mockeventTypeCache
                .Setup(x => x.Get(BusName.Kafka, Finality.Consume, ServerName.Default, topicName))
                .Returns(new List<Type> { typeof(FakeEvent) });

            return mockeventTypeCache;
        }

        [Theory]
        [MemberData(nameof(ConstructorData))]
        internal void ConstructorShouldThrowArgumentNullExceptionIfBusLoggerIsNull(IKafkaConsumerBuilder kafkaConsumerBuilder, IServiceProvider serviceProvider, IEventTypeCache cache, IEventMiddlewareExecutorProvider middlewareManager, IBusSerializerManager busSerializerManager, IBusLogDispatcher log)
        {
            Assert.Throws<ArgumentNullException>(() => new KafkaTopicEventConsumer(kafkaConsumerBuilder, serviceProvider, cache, middlewareManager, busSerializerManager, log));
        }

        [Fact]
        public void StartConsumingSubscribesToCorrectTopic()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                const string expectedTopic = "fake-events";

                var stubCache = CreateeventTypeTopicCache(expectedTopic);
                var stubMediator = Mock.Of<IMediator>();
                var serviceProvider = BuildServiceProvider(stubMediator);
                var stubEventConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                var mockConsumer = new Mock<IConsumer<string, byte[]>>();
                // throw exception to avoid infinite loop
                mockConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Throws<OperationCanceledException>();
                stubEventConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(mockConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)))
                    .Returns(JsonEventSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeEvent>(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var cts = new CancellationTokenSource();
                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, expectedTopic, cts.Token));
                Thread.Sleep(50);
                cts.Cancel();
                task.Wait();

                mockConsumer.Verify(x => x.Subscribe(expectedTopic));

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusExceptionEvent>(), It.IsAny<CancellationToken>()), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusTypedExceptionEvent<FakeEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public void StartConsumingConsumesEventFromConsumer()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var stubCache = CreateeventTypeTopicCache(FakeEvent.TOPIC_NAME);
                var stubMediator = Mock.Of<IMediator>();
                var serviceProvider = BuildServiceProvider(stubMediator);
                var stubEventConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                var mockConsumer = new Mock<IConsumer<string, byte[]>>();
                stubEventConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(mockConsumer.Object);
                // throw exception to avoid infinite loop
                mockConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Throws<OperationCanceledException>();

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)))
                    .Returns(JsonEventSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeEvent>(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var cts = new CancellationTokenSource();
                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cts.Token));
                Thread.Sleep(50);
                cts.Cancel();
                task.Wait();

                mockConsumer.Verify(x => x.Consume(It.IsAny<CancellationToken>()));

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusExceptionEvent>(), It.IsAny<CancellationToken>()), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusTypedExceptionEvent<FakeEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public void StartConsumingClosesConsumerWhenCancelled()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var stubCache = CreateeventTypeTopicCache(FakeEvent.TOPIC_NAME);
                var stubMediator = Mock.Of<IMediator>();
                var serviceProvider = BuildServiceProvider(stubMediator);
                var stubEventConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                var mockConsumer = new Mock<IConsumer<string, byte[]>>();
                mockConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Throws<OperationCanceledException>();
                stubEventConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(mockConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)))
                    .Returns(JsonEventSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeEvent>(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var cts = new CancellationTokenSource();
                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cts.Token));
                Thread.Sleep(50);
                cts.Cancel();
                task.Wait();

                mockConsumer.Verify(x => x.Close());

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusExceptionEvent>(), It.IsAny<CancellationToken>()), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusTypedExceptionEvent<FakeEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public void StartConsumingWithSerializerThrowingAnExceptionLogThisException()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var fakeEvent = new FakeEvent("some-key-id", "some-property-value");
                var cancellationTokenSource = new CancellationTokenSource();
                var mockMediator = new Mock<IMediator>();
                var serviceProvider = BuildServiceProvider(mockMediator.Object);
                var stubCache = CreateeventTypeTopicCache(FakeEvent.TOPIC_NAME);
                var stubConsumer = new Mock<IConsumer<string, byte[]>>();
                stubConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Returns(() =>
                    {
                        cancellationTokenSource.Cancel();
                        return BuildFakeConsumeResult(fakeEvent);
                    }); ;
                var stubEventConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubEventConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqDefaultSerializer = new Mock<IDefaultSerializer>();
                moqDefaultSerializer
                    .Setup(x => x.Deserialize(It.IsAny<Type>(), It.IsAny<byte[]>()))
                    .Throws<Exception>();

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)))
                    .Returns(moqDefaultSerializer.Object);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeEvent>(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cancellationTokenSource.Token));
                task.Wait();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusExceptionEvent>(), It.IsAny<CancellationToken>()), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusTypedExceptionEvent<FakeEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public void StartConsumingWithSerializerReturningNullLogEvent()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var fakeEvent = new FakeEvent("some-key-id", "some-property-value");
                var cancellationTokenSource = new CancellationTokenSource();
                var mockMediator = new Mock<IMediator>();
                var serviceProvider = BuildServiceProvider(mockMediator.Object);
                var stubCache = CreateeventTypeTopicCache(FakeEvent.TOPIC_NAME);
                var stubConsumer = new Mock<IConsumer<string, byte[]>>();
                stubConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Returns(() =>
                    {
                        cancellationTokenSource.Cancel();
                        return BuildFakeConsumeResult(fakeEvent);
                    }); ;
                var stubEventConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubEventConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)))
                    .Returns(Mock.Of<IDefaultSerializer>());

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeEvent>(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cancellationTokenSource.Token));
                task.Wait();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusLogEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [Fact]
        public void StartConsumingPublishesConsumedEventToMediator()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var fakeEvent = new FakeEvent("some-key-id", "some-property-value");
                var cancellationTokenSource = new CancellationTokenSource();
                var mockMediator = new Mock<IMediator>();
                var serviceProvider = BuildServiceProvider(mockMediator.Object);
                var stubCache = CreateeventTypeTopicCache(FakeEvent.TOPIC_NAME);
                var stubConsumer = new Mock<IConsumer<string, byte[]>>();
                stubConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Returns(() =>
                    {
                        cancellationTokenSource.Cancel();
                        return BuildFakeConsumeResult(fakeEvent);
                    }); ;
                var stubEventConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubEventConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)))
                    .Returns(JsonEventSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeEvent>(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cancellationTokenSource.Token));
                task.Wait();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)), Times.Once);
            }
        }

        [Fact]
        public void StartConsumingPublishesConsumedEventToMediatorWithMiddleware()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var middlewareExecuted = false;

                var fakeEvent = new FakeEvent("some-key-id", "some-property-value");
                var cancellationTokenSource = new CancellationTokenSource();
                var mockMediator = new Mock<IMediator>();

                var serviceProvider = BuildServiceProviderWithMiddleware<FakeEvent>(mockMediator.Object, @event =>
                {
                    middlewareExecuted = true;
                });

                var stubCache = CreateeventTypeTopicCache(FakeEvent.TOPIC_NAME);
                var stubConsumer = new Mock<IConsumer<string, byte[]>>();
                stubConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Returns(() =>
                    {
                        cancellationTokenSource.Cancel();
                        return BuildFakeConsumeResult(fakeEvent);
                    });
                var stubEventConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubEventConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)))
                    .Returns(JsonEventSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeEvent>(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cancellationTokenSource.Token));

                task.Wait();

                middlewareExecuted.Should().BeTrue();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)), Times.Once);
            }
        }

        [Fact]
        public void StartConsumingPublishesConsumedEventToMediatorWithMiddlewareThrowingAnException()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var middlewareExecuted = false;

                var fakeEvent = new FakeEvent("some-key-id", "some-property-value");
                var cancellationTokenSource = new CancellationTokenSource();
                var mockMediator = new Mock<IMediator>();

                var serviceProvider = BuildServiceProviderWithMiddleware<FakeEvent>(mockMediator.Object, @event =>
                {
                    middlewareExecuted = true;
                    throw new Exception();
                });

                var stubCache = CreateeventTypeTopicCache(FakeEvent.TOPIC_NAME);
                var stubConsumer = new Mock<IConsumer<string, byte[]>>();
                stubConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Returns(() =>
                    {
                        cancellationTokenSource.Cancel();
                        return BuildFakeConsumeResult(fakeEvent, true);
                    }); ;
                var stubEventConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubEventConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)))
                    .Returns(JsonEventSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeEvent>(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);
                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cancellationTokenSource.Token));

                task.Wait();

                mockMediator.Verify(x =>
                    x.Publish(
                        It.Is<object>(i => i.GetType() == typeof(FakeEvent)),
                        It.IsAny<CancellationToken>()), Times.Never);

                middlewareExecuted.Should().BeTrue();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusExceptionEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [Fact]
        public void StartConsumingPublishesConsumedEventToMediatorWithMiddlewareThrowingOperationCanceledException()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var middlewareExecuted = false;

                var fakeEvent = new FakeEvent("some-key-id", "some-property-value");
                var cancellationTokenSource = new CancellationTokenSource();
                var mockMediator = new Mock<IMediator>();

                var serviceProvider = BuildServiceProviderWithMiddleware<FakeEvent>(mockMediator.Object, @event =>
                {
                    middlewareExecuted = true;
                    throw new OperationCanceledException();
                });

                var stubCache = CreateeventTypeTopicCache(FakeEvent.TOPIC_NAME);
                var stubConsumer = new Mock<IConsumer<string, byte[]>>();
                stubConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Returns(() =>
                    {
                        cancellationTokenSource.Cancel();
                        return BuildFakeConsumeResult(fakeEvent);
                    }); ;
                var stubEventConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubEventConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)))
                    .Returns(JsonEventSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeEvent>(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);
                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cancellationTokenSource.Token));

                task.Wait();

                mockMediator.Verify(x =>
                    x.Publish(
                        It.Is<object>(i => i.GetType() == typeof(FakeEvent)),
                        It.IsAny<CancellationToken>()), Times.Never);

                middlewareExecuted.Should().BeTrue();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)), Times.Once);
            }
        }

        [Fact]
        public void StartConsumingPublishesWithTopicCacheThrowsExceptionConsumedEventToMediator()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var fakeEvent = new FakeEvent("some-key-id", "some-property-value");
                var cancellationTokenSource = new CancellationTokenSource();
                var mockMediator = new Mock<IMediator>();
                var serviceProvider = BuildServiceProvider(mockMediator.Object);
                var stubCache = CreateeventTypeTopicCache(FakeEvent.TOPIC_NAME + "s");
                stubCache
                    .Setup(cache => cache.Get(BusName.Kafka, Finality.Consume, ServerName.Default, FakeEvent.TOPIC_NAME))
                    .Throws<KeyNotFoundException>();

                var stubConsumer = new Mock<IConsumer<string, byte[]>>();
                stubConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Returns(() =>
                    {
                        cancellationTokenSource.Cancel();
                        return BuildFakeConsumeResult(fakeEvent);
                    });
                var stubEventConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubEventConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)))
                    .Returns(JsonEventSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeEvent>(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                Assert.Throws<KeyNotFoundException>(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cancellationTokenSource.Token));

                stubCache
                    .Verify(x => x.Get(BusName.Kafka, Finality.Consume, ServerName.Default, FakeEvent.TOPIC_NAME), Times.Once);

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)), Times.Never);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusExceptionEvent>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public void StartConsumingPublishesInvalidEventConsumedEventToMediator()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var fakeEvent = new FakeEvent("some-key-id", "some-property-value");
                var cancellationTokenSource = new CancellationTokenSource();
                var mockMediator = new Mock<IMediator>();
                var serviceProvider = BuildServiceProvider(mockMediator.Object);
                var stubCache = CreateeventTypeTopicCache(FakeEvent.TOPIC_NAME);
                var stubConsumer = new Mock<IConsumer<string, byte[]>>();
                stubConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Returns(() =>
                    {
                        cancellationTokenSource.Cancel();
                        return BuildFakeConsumeWithInvalidEventResult();
                    });
                var stubEventConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubEventConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)))
                    .Returns(JsonEventSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeEvent>(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cancellationTokenSource.Token)).Wait();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusExceptionEvent>(), It.IsAny<CancellationToken>()), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.Is<object>((obj, _) => obj.GetType().Equals(typeof(BusTypedExceptionEvent<FakeEvent>))), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        private static ServiceProvider BuildServiceProvider(IMediator mediator)
        {
            var serviceCollection = new ServiceCollection();
            var stubMiddlewareManager = new Mock<IEventMiddlewareExecutorProvider>();
            serviceCollection.AddSingleton<MediatorPublisherMiddleware<FakeEvent>>();
            stubMiddlewareManager.Setup(x => x.GetPipeline(It.IsAny<BusName>(), It.IsAny<Enum>(), It.IsAny<Type>())).Returns(new Pipeline(new List<Type> { typeof(MediatorPublisherMiddleware<FakeEvent>) }, typeof(FakeEvent)));
            serviceCollection.AddScoped(s => mediator);
            serviceCollection.AddSingleton(s => stubMiddlewareManager.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }

        private static ServiceProvider BuildServiceProviderWithMiddleware<TeventType>(IMediator mediator, Action<TeventType> onMiddlewareExecuted)
            where TeventType : IEvent
        {
            var stubMiddlewareManager = new Mock<IEventMiddlewareExecutorProvider>();
            stubMiddlewareManager.Setup(x => x.GetPipeline(It.IsAny<BusName>(), It.IsAny<Enum>(), It.IsAny<Type>())).Returns(new Pipeline(new List<Type> { typeof(FakeEventMiddlewareWithFuncOnConstructor), typeof(MediatorPublisherMiddleware<FakeEvent>) }, typeof(FakeEvent)));

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(_ => new FakeEventMiddlewareWithFuncOnConstructor(context =>
            {
                onMiddlewareExecuted((context as ConsumeContext<TeventType>).Event);
                return Task.CompletedTask;
            }));
            serviceCollection.AddSingleton<MediatorPublisherMiddleware<FakeEvent>>();
            serviceCollection.AddSingleton(s => stubMiddlewareManager.Object);
            serviceCollection.AddScoped(s => mediator);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }

        private static ConsumeResult<string, byte[]> BuildFakeConsumeResult(FakeEvent fakeEvent, bool nullHeader = false)
        {
            return new ConsumeResult<string, byte[]>
            {
                Message = new Message<string, byte[]>
                {
                    Value = JsonSerializer.SerializeToUtf8Bytes(fakeEvent),
                    Headers = !nullHeader ? new Headers
                    {
                        {"event-type", Encoding.UTF8.GetBytes(fakeEvent.GetType().AssemblyQualifiedName)}
                    } : null
                }
            };
        }

        private static ConsumeResult<string, byte[]> BuildFakeConsumeWithInvalidEventResult()
        {
            return new ConsumeResult<string, byte[]>
            {
                Message = new Message<string, byte[]>
                {
                    Value = Encoding.UTF8.GetBytes("{"),
                    Headers = new Headers
                    {
                        {"event-type", Encoding.UTF8.GetBytes(typeof(FakeEvent).AssemblyQualifiedName)}
                    }
                }
            };
        }
    }
}