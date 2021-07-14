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
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Kafka.Common;
using GuimoSoft.Bus.Kafka.Consumer;
using GuimoSoft.Bus.Tests.Fakes;
using GuimoSoft.Serialization;
using GuimoSoft.Serialization.Interfaces;
using Xunit;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class KafkaTopicMessageConsumerTests
    {
        private static Mock<IKafkaTopicCache> CreateKafkaTopicCache()
        {
            var mockCache = new Mock<IKafkaTopicCache>();
            mockCache.SetupGet(cache => cache[typeof(FakeMessage)]).Returns(FakeMessage.TOPIC_NAME);
            mockCache.SetupGet(cache => cache[typeof(OtherFakeMessage)]).Returns(OtherFakeMessage.TOPIC_NAME);
            mockCache.SetupGet(cache => cache[typeof(AnotherFakeMessage)]).Returns(AnotherFakeMessage.TOPIC_NAME);

            mockCache.SetupGet(cache => cache[FakeMessage.TOPIC_NAME]).Returns(new List<Type> { typeof(FakeMessage) });
            mockCache.SetupGet(cache => cache[OtherFakeMessage.TOPIC_NAME]).Returns(new List<Type> { typeof(OtherFakeMessage) });
            mockCache.SetupGet(cache => cache[AnotherFakeMessage.TOPIC_NAME]).Returns(new List<Type> { typeof(AnotherFakeMessage) });

            return mockCache;
        }

        [Fact]
        public void StartConsumingSubscribesToCorrectTopic()
        {
            const string expectedTopic = "fake-messages";

            var stubCache = CreateKafkaTopicCache();
            var stubMediator = Mock.Of<IMediator>();
            var serviceProvider = BuildServiceProvider(stubMediator);
            var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
            var mockConsumer = new Mock<IConsumer<string, byte[]>>();
            // throw exception to avoid infinite loop
            mockConsumer
                .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                .Throws<OperationCanceledException>();
            stubMessageConsumerBuilder
                .Setup(x => x.Build())
                .Returns(mockConsumer.Object);

            var moqSerializerManager = new Mock<IMessageSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IMessageMiddlewareExecutorProvider>(), moqSerializerManager.Object);

            var cts = new CancellationTokenSource();
            var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(expectedTopic, cts.Token));
            Thread.Sleep(50);
            cts.Cancel();
            task.Wait();

            mockConsumer.Verify(x => x.Subscribe(expectedTopic));
        }

        [Fact]
        public void StartConsumingConsumesMessageFromConsumer()
        {
            var stubCache = CreateKafkaTopicCache();
            var stubMediator = Mock.Of<IMediator>();
            var serviceProvider = BuildServiceProvider(stubMediator);
            var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
            var mockConsumer = new Mock<IConsumer<string, byte[]>>();
            stubMessageConsumerBuilder
                .Setup(x => x.Build())
                .Returns(mockConsumer.Object);
            // throw exception to avoid infinite loop
            mockConsumer
                .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                .Throws<OperationCanceledException>();
            
            var moqSerializerManager = new Mock<IMessageSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IMessageMiddlewareExecutorProvider>(), moqSerializerManager.Object);

            var cts = new CancellationTokenSource();
            var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(FakeMessage.TOPIC_NAME, cts.Token));
            Thread.Sleep(50);
            cts.Cancel();
            task.Wait();

            mockConsumer.Verify(x => x.Consume(It.IsAny<CancellationToken>()));
        }

        [Fact]
        public void StartConsumingClosesConsumerWhenCancelled()
        {
            var stubCache = CreateKafkaTopicCache();
            var stubMediator = Mock.Of<IMediator>();
            var serviceProvider = BuildServiceProvider(stubMediator);
            var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
            var mockConsumer = new Mock<IConsumer<string, byte[]>>();
            mockConsumer
                .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                .Throws<OperationCanceledException>();
            stubMessageConsumerBuilder
                .Setup(x => x.Build())
                .Returns(mockConsumer.Object);

            var moqSerializerManager = new Mock<IMessageSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IMessageMiddlewareExecutorProvider>(), moqSerializerManager.Object);

            var cts = new CancellationTokenSource();
            var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(FakeMessage.TOPIC_NAME, cts.Token));
            Thread.Sleep(50);
            cts.Cancel();
            task.Wait();

            mockConsumer.Verify(x => x.Close());
        }

        [Fact]
        public void StartConsumingPublishesConsumedMessageToMediator()
        {
            var fakeMessage = new FakeMessage("some-key-id", "some-property-value");
            var fakeMessageNotification = new MessageNotification<FakeMessage>(fakeMessage);
            var cancellationTokenSource = new CancellationTokenSource();
            var mockMediator = new Mock<IMediator>();
            var serviceProvider = BuildServiceProvider(mockMediator.Object);
            var stubCache = CreateKafkaTopicCache();
            var stubConsumer = new Mock<IConsumer<string, byte[]>>();
            stubConsumer
                .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    cancellationTokenSource.Cancel();
                    return BuildFakeConsumeResult(fakeMessage);
                }); ;
            var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
            stubMessageConsumerBuilder
                .Setup(x => x.Build())
                .Returns(stubConsumer.Object);

            var moqSerializerManager = new Mock<IMessageSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            // TODO: find better way to test than relying on async timing
            var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IMessageMiddlewareExecutorProvider>(), moqSerializerManager.Object);
            var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));
            task.Wait();

            moqSerializerManager
                .Verify(x => x.GetSerializer(typeof(FakeMessage)), Times.Once);

            mockMediator.Verify(x =>
                x.Publish(
                    fakeMessageNotification,
                    It.IsAny<CancellationToken>()));
        }

        [Fact]
        public void StartConsumingPublishesConsumedMessageToMediatorWithMiddleware()
        {
            var middlewareExecuted = false;

            var fakeMessage = new FakeMessage("some-key-id", "some-property-value");
            var fakeMessageNotification = new MessageNotification<FakeMessage>(fakeMessage);
            var cancellationTokenSource = new CancellationTokenSource();
            var mockMediator = new Mock<IMediator>();

            var serviceProvider = BuildServiceProviderWithMiddleware<FakeMessage>(mockMediator.Object, message =>
            {
                middlewareExecuted = true;
            });

            var stubCache = CreateKafkaTopicCache();
            var stubConsumer = new Mock<IConsumer<string, byte[]>>();
            stubConsumer
                .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    cancellationTokenSource.Cancel();
                    return BuildFakeConsumeResult(fakeMessage);
                });
            var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
            stubMessageConsumerBuilder
                .Setup(x => x.Build())
                .Returns(stubConsumer.Object);

            var moqSerializerManager = new Mock<IMessageSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            // TODO: find better way to test than relying on async timing
            var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IMessageMiddlewareExecutorProvider>(), moqSerializerManager.Object);
            var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));

            task.Wait();

            mockMediator.Verify(x =>
                x.Publish(
                    fakeMessageNotification,
                    It.IsAny<CancellationToken>()));

            middlewareExecuted.Should().BeTrue();

            moqSerializerManager
                .Verify(x => x.GetSerializer(typeof(FakeMessage)), Times.Once);
        }

        [Fact]
        public void StartConsumingPublishesConsumedMessageToMediatorWithMiddlewareThrowingAnException()
        {
            var middlewareExecuted = false;

            var fakeMessage = new FakeMessage("some-key-id", "some-property-value");
            var cancellationTokenSource = new CancellationTokenSource();
            var mockMediator = new Mock<IMediator>();

            var serviceProvider = BuildServiceProviderWithMiddleware<FakeMessage>(mockMediator.Object, message =>
            {
                middlewareExecuted = true;
                throw new Exception();
            });

            var stubCache = CreateKafkaTopicCache();
            var stubConsumer = new Mock<IConsumer<string, byte[]>>();
            stubConsumer
                .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    cancellationTokenSource.Cancel();
                    return BuildFakeConsumeResult(fakeMessage);
                }); ;
            var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
            stubMessageConsumerBuilder
                .Setup(x => x.Build())
                .Returns(stubConsumer.Object);

            var moqSerializerManager = new Mock<IMessageSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            // TODO: find better way to test than relying on async timing
            var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IMessageMiddlewareExecutorProvider>(), moqSerializerManager.Object);
            var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));

            task.Wait();

            mockMediator.Verify(x =>
                x.Publish(
                    It.Is<object>(i => i.GetType() == typeof(MessageNotification<FakeMessage>)),
                    It.IsAny<CancellationToken>()), Times.Never);

            middlewareExecuted.Should().BeTrue();

            moqSerializerManager
                .Verify(x => x.GetSerializer(typeof(FakeMessage)), Times.Once);
        }

        [Fact]
        public void StartConsumingPublishesWithTopicCacheThrowsExceptionConsumedMessageToMediator()
        {
            var fakeMessage = new FakeMessage("some-key-id", "some-property-value");
            var cancellationTokenSource = new CancellationTokenSource();
            var mockMediator = new Mock<IMediator>();
            var serviceProvider = BuildServiceProvider(mockMediator.Object);
            var stubCache = CreateKafkaTopicCache();
            stubCache.SetupGet(cache => cache[FakeMessage.TOPIC_NAME]).Throws<KeyNotFoundException>();

            var stubConsumer = new Mock<IConsumer<string, byte[]>>();
            stubConsumer
                .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    cancellationTokenSource.Cancel();
                    return BuildFakeConsumeResult(fakeMessage);
                });
            var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
            stubMessageConsumerBuilder
                .Setup(x => x.Build())
                .Returns(stubConsumer.Object);

            var moqSerializerManager = new Mock<IMessageSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            // TODO: find better way to test than relying on async timing
            var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IMessageMiddlewareExecutorProvider>(), moqSerializerManager.Object);
            var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));

            task.Wait();

            mockMediator.Verify(x =>
                x.Publish(
                    It.Is<object>(i => i.GetType() == typeof(MessageNotification<FakeMessage>)),
                    It.IsAny<CancellationToken>()), Times.Never);

            moqSerializerManager
                .Verify(x => x.GetSerializer(typeof(FakeMessage)), Times.Never);
        }

        [Fact]
        public async Task StartConsumingPublishesInvalidMessageConsumedMessageToMediator()
        {
            var fakeMessage = new FakeMessage("some-key-id", "some-property-value");
            var cancellationTokenSource = new CancellationTokenSource();
            var mockMediator = new Mock<IMediator>();
            var serviceProvider = BuildServiceProvider(mockMediator.Object);
            var stubCache = CreateKafkaTopicCache();
            var stubConsumer = new Mock<IConsumer<string, byte[]>>();
            stubConsumer
                .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    cancellationTokenSource.Cancel();
                    return BuildFakeConsumeWithInvalidMessageResult();
                });
            var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
            stubMessageConsumerBuilder
                .Setup(x => x.Build())
                .Returns(stubConsumer.Object);

            var moqSerializerManager = new Mock<IMessageSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            // TODO: find better way to test than relying on async timing
            var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IMessageMiddlewareExecutorProvider>(), moqSerializerManager.Object);
            _ = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));
            await Task.Delay(500);
            cancellationTokenSource.Cancel();

            mockMediator.Verify(x =>
                x.Publish(
                    It.Is<object>(i => i.GetType() == typeof(MessageNotification<FakeMessage>)),
                    It.IsAny<CancellationToken>()), Times.Never);

            moqSerializerManager
                .Verify(x => x.GetSerializer(typeof(FakeMessage)), Times.Once);
        }

        private static ServiceProvider BuildServiceProvider(IMediator mediator)
        {
            var serviceCollection = new ServiceCollection();
            var stubMiddlewareManager = new Mock<IMessageMiddlewareExecutorProvider>();
            serviceCollection.AddSingleton<MediatorPublisherMiddleware<FakeMessage>>();
            stubMiddlewareManager.Setup(x => x.GetPipeline(It.IsAny<Type>())).Returns(new Pipeline(new List<Type> { typeof(MediatorPublisherMiddleware<FakeMessage>) }));
            serviceCollection.AddScoped(s => mediator);
            serviceCollection.AddSingleton(s => stubMiddlewareManager.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }

        private static ServiceProvider BuildServiceProviderWithMiddleware<TMessageType>(IMediator mediator, Action<TMessageType> onMiddlewareExecuted)
            where TMessageType : IMessage
        {
            var stubMiddlewareManager = new Mock<IMessageMiddlewareExecutorProvider>();
            stubMiddlewareManager.Setup(x => x.GetPipeline(It.IsAny<Type>())).Returns(new Pipeline(new List<Type> { typeof(FakeMessageMiddlewareWithFuncOnConstructor), typeof(MediatorPublisherMiddleware<FakeMessage>) }));

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(_ => new FakeMessageMiddlewareWithFuncOnConstructor(context =>
            {
                onMiddlewareExecuted((context as ConsumptionContext<TMessageType>).Message);
                return Task.CompletedTask;
            }));
            serviceCollection.AddSingleton<MediatorPublisherMiddleware<FakeMessage>>();
            serviceCollection.AddSingleton(s => stubMiddlewareManager.Object);
            serviceCollection.AddScoped(s => mediator);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }

        private static ConsumeResult<string, byte[]> BuildFakeConsumeResult(FakeMessage fakeMessage)
        {
            return new ConsumeResult<string, byte[]>
            {
                Message = new Message<string, byte[]>
                {
                    Value = JsonSerializer.SerializeToUtf8Bytes(fakeMessage),
                    Headers = new Headers
                    {
                        {"message-type", Encoding.UTF8.GetBytes(fakeMessage.GetType().AssemblyQualifiedName)}
                    }
                }
            };
        }

        private static ConsumeResult<string, byte[]> BuildFakeConsumeWithInvalidMessageResult()
        {
            return new ConsumeResult<string, byte[]>
            {
                Message = new Message<string, byte[]>
                {
                    Value = Encoding.UTF8.GetBytes("{"),
                    Headers = new Headers
                    {
                        {"message-type", Encoding.UTF8.GetBytes(typeof(FakeMessage).AssemblyQualifiedName)}
                    }
                }
            };
        }
    }
}