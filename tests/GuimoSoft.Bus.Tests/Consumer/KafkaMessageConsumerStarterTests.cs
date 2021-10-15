using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Kafka.Consumer;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class KafkaEventConsumerStarterTests
    {
        [Fact]
        public void StartConsumersShouldStartSingleConsumerPerEvent()
        {
            var mockeventTypeCache = new Mock<IEventTypeCache>();
            mockeventTypeCache
                .Setup(x => x.GetSwitchers(BusName.Kafka, Finality.Consume))
                .Returns(new List<Enum> { ServerName.Default });
            mockeventTypeCache
                .Setup(x => x.GetEndpoints(BusName.Kafka, Finality.Consume, ServerName.Default))
                .Returns(new List<string> { FakeEvent.TOPIC_NAME, OtherFakeEvent.TOPIC_NAME, AnotherFakeEvent.TOPIC_NAME });

            var mockKafkaEventConsumer = new Mock<IKafkaTopicEventConsumer>();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(mockKafkaEventConsumer.Object);
            serviceCollection.AddSingleton(s => mockeventTypeCache.Object);
            serviceCollection.AddTransient(s => Mock.Of<INotificationHandler<FakeEvent>>());
            serviceCollection.AddTransient(s => Mock.Of<INotificationHandler<OtherFakeEvent>>());
            serviceCollection.AddTransient(s => Mock.Of<INotificationHandler<AnotherFakeEvent>>());
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var sut = new KafkaEventConsumerManager(serviceProvider);
            sut.StartConsumers(CancellationToken.None);

            mockKafkaEventConsumer.Verify(x => x.ConsumeUntilCancellationIsRequested(It.IsAny<Enum>(), FakeEvent.TOPIC_NAME, It.IsAny<CancellationToken>()),
                Times.Once);
            mockKafkaEventConsumer.Verify(x => x.ConsumeUntilCancellationIsRequested(It.IsAny<Enum>(), OtherFakeEvent.TOPIC_NAME, It.IsAny<CancellationToken>()),
                Times.Once);
            mockKafkaEventConsumer.Verify(x => x.ConsumeUntilCancellationIsRequested(It.IsAny<Enum>(), AnotherFakeEvent.TOPIC_NAME, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}