using Confluent.Kafka;
using FluentAssertions;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Core.Internal.Middlewares;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Bus.Core.Logs.Builder;
using GuimoSoft.Bus.Kafka.Consumer;
using GuimoSoft.Bus.Tests.Fakes;
using GuimoSoft.Core.Serialization;
using GuimoSoft.Core.Serialization.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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

        private static (Mock<IBusLogDispatcher>, Mock<IServiceProvider>) CreateLoggerMock(BusName bus)
        {
            var moqServiceProvider = new Mock<IServiceProvider>();

            moqServiceProvider
                .Setup(x => x.GetService(typeof(EventDispatcherMiddleware<BusLogEvent>)))
                .Returns(new EventDispatcherMiddleware<BusLogEvent>());

            moqServiceProvider
                .Setup(x => x.GetService(typeof(EventDispatcherMiddleware<BusExceptionEvent>)))
                .Returns(new EventDispatcherMiddleware<BusExceptionEvent>());

            var logBuilder = new BusLogDispatcherBuilder(moqServiceProvider.Object, bus);

            var moqLogger = new Mock<IBusLogDispatcher>();

            moqLogger
                .Setup(x => x.FromBus(bus)).Returns(logBuilder);

            return (moqLogger, moqServiceProvider);
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
                var serviceProvider = BuildServiceProvider();
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

                var (moqLogger, moqServiceProvider) = CreateLoggerMock(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var cts = new CancellationTokenSource();
                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, expectedTopic, cts.Token));
                Thread.Sleep(50);
                cts.Cancel();
                task.Wait();

                mockConsumer.Verify(x => x.Subscribe(expectedTopic));

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusExceptionEvent>)), Times.Once);

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusTypedExceptionEvent<FakeEvent>>)), Times.Never);
            }
        }

        [Fact]
        public void StartConsumingConsumesEventFromConsumer()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var stubCache = CreateeventTypeTopicCache(FakeEvent.TOPIC_NAME);
                var serviceProvider = BuildServiceProvider();
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

                var (moqLogger, moqServiceProvider) = CreateLoggerMock(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var cts = new CancellationTokenSource();
                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cts.Token));
                Thread.Sleep(50);
                cts.Cancel();
                task.Wait();

                mockConsumer.Verify(x => x.Consume(It.IsAny<CancellationToken>()));

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusExceptionEvent>)), Times.Once);

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusTypedExceptionEvent<FakeEvent>>)), Times.Never);
            }
        }

        [Fact]
        public void StartConsumingClosesConsumerWhenCancelled()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var stubCache = CreateeventTypeTopicCache(FakeEvent.TOPIC_NAME);
                var serviceProvider = BuildServiceProvider();
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

                var (moqLogger, moqServiceProvider) = CreateLoggerMock(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var cts = new CancellationTokenSource();
                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cts.Token));
                Thread.Sleep(50);
                cts.Cancel();
                task.Wait();

                mockConsumer.Verify(x => x.Close());

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusExceptionEvent>)), Times.Once);

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusTypedExceptionEvent<FakeEvent>>)), Times.Never);
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
                var serviceProvider = BuildServiceProvider();
                var stubCache = CreateeventTypeTopicCache(FakeEvent.TOPIC_NAME);
                var stubConsumer = new Mock<IConsumer<string, byte[]>>();
                _ = stubConsumer
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

                var moqDefaultSerializer = new Mock<IDefaultSerializer>();
                moqDefaultSerializer
                    .Setup(x => x.Deserialize(It.IsAny<Type>(), It.IsAny<byte[]>()))
                    .Throws<Exception>();

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)))
                    .Returns(moqDefaultSerializer.Object);

                var (moqLogger, moqServiceProvider) = CreateLoggerMock(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cancellationTokenSource.Token));
                task.Wait();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)), Times.Once);

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusExceptionEvent>)), Times.Once);

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusTypedExceptionEvent<FakeEvent>>)), Times.Never);
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
                var serviceProvider = BuildServiceProvider();
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
                    .Returns(Mock.Of<IDefaultSerializer>());

                var (moqLogger, moqServiceProvider) = CreateLoggerMock(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cancellationTokenSource.Token));
                task.Wait();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)), Times.Once);

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusLogEvent>)), Times.Once);
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
                var serviceProvider = BuildServiceProvider();
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

                var (moqLogger, moqMediator) = CreateLoggerMock(BusName.Kafka);

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

                var serviceProvider = BuildServiceProviderWithMiddleware<FakeEvent>(@event =>
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

                var (moqLogger, moqMediator) = CreateLoggerMock(BusName.Kafka);

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

                var serviceProvider = BuildServiceProviderWithMiddleware<FakeEvent>(@event =>
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
                    });
                var stubEventConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubEventConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)))
                    .Returns(JsonEventSerializer.Instance);

                var (moqLogger, moqServiceProvider) = CreateLoggerMock(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);
                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cancellationTokenSource.Token));

                task.Wait();

                middlewareExecuted.Should().BeTrue();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)), Times.Once);

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusExceptionEvent>)), Times.Once);
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

                var serviceProvider = BuildServiceProviderWithMiddleware<FakeEvent>(@event =>
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
                    });
                var stubEventConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubEventConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)))
                    .Returns(JsonEventSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);
                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cancellationTokenSource.Token));

                task.Wait();

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
                var serviceProvider = BuildServiceProvider();
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

                var (moqLogger, moqServiceProvider) = CreateLoggerMock(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                Assert.Throws<KeyNotFoundException>(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cancellationTokenSource.Token));

                stubCache
                    .Verify(x => x.Get(BusName.Kafka, Finality.Consume, ServerName.Default, FakeEvent.TOPIC_NAME), Times.Once);

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)), Times.Never);

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusExceptionEvent>)), Times.Never);
            }
        }

        [Fact]
        public void StartConsumingPublishesInvalidEventConsumedEventToMediator()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var cancellationTokenSource = new CancellationTokenSource();
                var serviceProvider = BuildServiceProvider();
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

                var (moqLogger, moqServiceProvider) = CreateLoggerMock(BusName.Kafka);

                var sut = new KafkaTopicEventConsumer(stubEventConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeEvent.TOPIC_NAME, cancellationTokenSource.Token)).Wait();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeEvent)), Times.Once);

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusExceptionEvent>)), Times.Once);

                moqServiceProvider
                    .Verify(x => x.GetService(typeof(EventDispatcherMiddleware<BusTypedExceptionEvent<FakeEvent>>)), Times.Never);
            }
        }

        private static ServiceProvider BuildServiceProvider()
        {
            var serviceCollection = new ServiceCollection();
            var stubMiddlewareManager = new Mock<IEventMiddlewareExecutorProvider>();
            serviceCollection.AddSingleton<EventDispatcherMiddleware<FakeEvent>>();
            stubMiddlewareManager.Setup(x => x.GetPipeline(It.IsAny<BusName>(), It.IsAny<Enum>(), It.IsAny<Type>())).Returns(new Pipeline(new List<Type> { typeof(EventDispatcherMiddleware<FakeEvent>) }, typeof(FakeEvent)));
            serviceCollection.AddSingleton(s => stubMiddlewareManager.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }

        private static ServiceProvider BuildServiceProviderWithMiddleware<TeventType>(Action<TeventType> onMiddlewareExecuted)
            where TeventType : IEvent
        {
            var stubMiddlewareManager = new Mock<IEventMiddlewareExecutorProvider>();
            stubMiddlewareManager.Setup(x => x.GetPipeline(It.IsAny<BusName>(), It.IsAny<Enum>(), It.IsAny<Type>())).Returns(new Pipeline(new List<Type> { typeof(FakeEventMiddlewareWithFuncOnConstructor), typeof(EventDispatcherMiddleware<FakeEvent>) }, typeof(FakeEvent)));

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(_ => new FakeEventMiddlewareWithFuncOnConstructor(context =>
            {
                onMiddlewareExecuted((context as ConsumeContext<TeventType>).Event);
                return Task.CompletedTask;
            }));
            serviceCollection.AddSingleton<EventDispatcherMiddleware<FakeEvent>>();
            serviceCollection.AddSingleton(s => stubMiddlewareManager.Object);
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