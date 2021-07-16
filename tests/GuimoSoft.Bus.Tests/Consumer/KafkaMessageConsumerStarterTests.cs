using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Kafka.Common;
using GuimoSoft.Bus.Kafka.Consumer;
using GuimoSoft.Bus.Tests.Fakes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class KafkaMessageConsumerStarterTests
    {
        private Mock<IKafkaTopicCache> CreateKafkaTopicCache()
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
        public void StartConsumersShouldStartSingleConsumerPerMessage()
        {
            var mockCache = CreateKafkaTopicCache();
            var mockKafkaMessageConsumer = new Mock<IKafkaTopicMessageConsumer>();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(mockCache.Object);
            serviceCollection.AddSingleton(mockKafkaMessageConsumer.Object);
            serviceCollection.AddTransient(s => Mock.Of<INotificationHandler<MessageNotification<FakeMessage>>>());
            serviceCollection.AddTransient(s => Mock.Of<INotificationHandler<MessageNotification<OtherFakeMessage>>>());
            serviceCollection.AddTransient(s => Mock.Of<INotificationHandler<MessageNotification<AnotherFakeMessage>>>());
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var sut = new KafkaMessageConsumerManager(serviceProvider, serviceCollection);
            sut.StartConsumers(CancellationToken.None);

            mockKafkaMessageConsumer.Verify(x => x.ConsumeUntilCancellationIsRequested(FakeMessage.TOPIC_NAME, It.IsAny<CancellationToken>()),
                Times.Once);
            mockKafkaMessageConsumer.Verify(x => x.ConsumeUntilCancellationIsRequested(OtherFakeMessage.TOPIC_NAME, It.IsAny<CancellationToken>()),
                Times.Once);
            mockKafkaMessageConsumer.Verify(x => x.ConsumeUntilCancellationIsRequested(AnotherFakeMessage.TOPIC_NAME, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}