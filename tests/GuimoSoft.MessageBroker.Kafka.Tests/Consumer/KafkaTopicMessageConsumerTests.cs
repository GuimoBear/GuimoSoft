using Confluent.Kafka;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.MessageBroker.Abstractions;
using GuimoSoft.MessageBroker.Abstractions.Consumer;
using GuimoSoft.MessageBroker.Kafka.Common;
using GuimoSoft.MessageBroker.Kafka.Consumer;
using GuimoSoft.MessageBroker.Kafka.Tests.Fakes;
using Xunit;

namespace GuimoSoft.MessageBroker.Kafka.Tests.Consumer
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
            var mockConsumer = new Mock<IConsumer<string, string>>();
            // throw exception to avoid infinite loop
            mockConsumer
                .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                .Throws<OperationCanceledException>();
            stubMessageConsumerBuilder
                .Setup(x => x.Build())
                .Returns(mockConsumer.Object);

            var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IMessageMiddlereExecutorProvider>());

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
            var mockConsumer = new Mock<IConsumer<string, string>>();
            stubMessageConsumerBuilder
                .Setup(x => x.Build())
                .Returns(mockConsumer.Object);
            // throw exception to avoid infinite loop
            mockConsumer
                .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                .Throws<OperationCanceledException>();

            var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IMessageMiddlereExecutorProvider>());

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
            var mockConsumer = new Mock<IConsumer<string, string>>();
            mockConsumer
                .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                .Throws<OperationCanceledException>();
            stubMessageConsumerBuilder
                .Setup(x => x.Build())
                .Returns(mockConsumer.Object);

            var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IMessageMiddlereExecutorProvider>());

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
            var cancellationTokenSource = new CancellationTokenSource();
            var mockMediator = new Mock<IMediator>();
            var serviceProvider = BuildServiceProvider(mockMediator.Object);
            var stubCache = CreateKafkaTopicCache();
            var stubConsumer = new Mock<IConsumer<string, string>>();
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

            // TODO: find better way to test than relying on async timing
            var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IMessageMiddlereExecutorProvider>());
            var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));
            task.Wait();

            mockMediator.Verify(x =>
                x.Publish(
                    It.Is<object>(i => i.GetType() == typeof(MessageNotification<FakeMessage>)),
                    It.IsAny<CancellationToken>()));
        }

        [Fact]
        public void StartConsumingPublishesConsumedMessageToMediatorWithMiddleware()
        {
            var middlewareExecuted = false;

            var fakeMessage = new FakeMessage("some-key-id", "some-property-value");
            var cancellationTokenSource = new CancellationTokenSource();
            var mockMediator = new Mock<IMediator>();

            var serviceProvider = BuildServiceProviderWithMiddleware<FakeMessage>(mockMediator.Object, message =>
            {
                middlewareExecuted = true;
            });

            var stubCache = CreateKafkaTopicCache();
            var stubConsumer = new Mock<IConsumer<string, string>>();
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

            // TODO: find better way to test than relying on async timing
            var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IMessageMiddlereExecutorProvider>());
            var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));

            task.Wait();

            mockMediator.Verify(x =>
                x.Publish(
                    It.Is<object>(i => i.GetType() == typeof(MessageNotification<FakeMessage>)),
                    It.IsAny<CancellationToken>()));

            middlewareExecuted.Should().BeTrue();
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
            var stubConsumer = new Mock<IConsumer<string, string>>();
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

            // TODO: find better way to test than relying on async timing
            var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IMessageMiddlereExecutorProvider>());
            var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));

            task.Wait();

            mockMediator.Verify(x =>
                x.Publish(
                    It.Is<object>(i => i.GetType() == typeof(MessageNotification<FakeMessage>)),
                    It.IsAny<CancellationToken>()), Times.Never);

            middlewareExecuted.Should().BeTrue();
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

            var stubConsumer = new Mock<IConsumer<string, string>>();
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

            // TODO: find better way to test than relying on async timing
            var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IMessageMiddlereExecutorProvider>());
            var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));

            task.Wait();

            mockMediator.Verify(x =>
                x.Publish(
                    It.Is<object>(i => i.GetType() == typeof(MessageNotification<FakeMessage>)),
                    It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task StartConsumingPublishesInvalidMessageConsumedMessageToMediator()
        {
            var fakeMessage = new FakeMessage("some-key-id", "some-property-value");
            var cancellationTokenSource = new CancellationTokenSource();
            var mockMediator = new Mock<IMediator>();
            var serviceProvider = BuildServiceProvider(mockMediator.Object);
            var stubCache = CreateKafkaTopicCache();
            var stubConsumer = new Mock<IConsumer<string, string>>();
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

            // TODO: find better way to test than relying on async timing
            var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IMessageMiddlereExecutorProvider>());
            _ = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));
            await Task.Delay(500);
            cancellationTokenSource.Cancel();

            mockMediator.Verify(x =>
                x.Publish(
                    It.Is<object>(i => i.GetType() == typeof(MessageNotification<FakeMessage>)),
                    It.IsAny<CancellationToken>()), Times.Never);
        }

        private static ServiceProvider BuildServiceProvider(IMediator mediator)
        {
            var serviceCollection = new ServiceCollection();
            var stubMiddlewareManager = new Mock<IMessageMiddlereExecutorProvider>();
            stubMiddlewareManager.Setup(x => x.GetPipeline(It.IsAny<Type>())).Returns(new Pipeline(new List<Type>()));
            serviceCollection.AddScoped(s => mediator);
            serviceCollection.AddSingleton(s => stubMiddlewareManager.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }

        private static ServiceProvider BuildServiceProviderWithMiddleware<TMessageType>(IMediator mediator, Action<TMessageType> onMiddlewareExecuted)
            where TMessageType : IMessage
        {
            var stubMiddlewareManager = new Mock<IMessageMiddlereExecutorProvider>();
            stubMiddlewareManager.Setup(x => x.GetPipeline(It.IsAny<Type>())).Returns(new Pipeline(new List<Type> { typeof(FakeMessageMiddlewareWithFuncOnConstructor) }));

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(_ => new FakeMessageMiddlewareWithFuncOnConstructor(context =>
            {
                onMiddlewareExecuted((context as ConsumptionContext<TMessageType>).Message);
                return Task.CompletedTask;
            }));
            serviceCollection.AddSingleton(s => stubMiddlewareManager.Object);
            serviceCollection.AddScoped(s => mediator);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }

        private static ConsumeResult<string, string> BuildFakeConsumeResult(FakeMessage fakeMessage)
        {
            return new ConsumeResult<string, string>
            {
                Message = new Message<string, string>
                {
                    Value = JsonConvert.SerializeObject(fakeMessage),
                    Headers = new Headers
                    {
                        {"message-type", Encoding.UTF8.GetBytes(fakeMessage.GetType().AssemblyQualifiedName)}
                    }
                }
            };
        }

        private static ConsumeResult<string, string> BuildFakeConsumeWithInvalidMessageResult()
        {
            return new ConsumeResult<string, string>
            {
                Message = new Message<string, string>
                {
                    Value = "{",
                    Headers = new Headers
                    {
                        {"message-type", Encoding.UTF8.GetBytes(typeof(FakeMessage).AssemblyQualifiedName)}
                    }
                }
            };
        }
    }
}