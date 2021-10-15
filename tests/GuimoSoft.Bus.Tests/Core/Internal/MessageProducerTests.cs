using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Utils;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Internal
{
    public class EventProducerTests
    {
        public static readonly IEnumerable<object[]> ConstructorInvalidData
            = new List<object[]>
            {
                new object[] { null, null, null },
                new object[] { Mock.Of<IDispatcherManager>(), null, null },
                new object[] { Mock.Of<IDispatcherManager>(), Mock.Of<IEventTypeCache>(), null },
            };

        [Theory]
        [MemberData(nameof(ConstructorInvalidData))]
        internal void ConstructorShouldThrowIfAnyParameterIsNull(IDispatcherManager producerManager, IEventTypeCache eventTypeCache, IServiceProvider services)
        {
            Assert.Throws<ArgumentNullException>(() => new EventBus(producerManager, eventTypeCache, services));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async Task ProduceAsyncShouldBeThrowArgumentExceptionIfKeyIsNullEmptyOrWriteSpace(string key)
        {
            var sut = new EventBus(Mock.Of<IDispatcherManager>(), Mock.Of<IEventTypeCache>(), Mock.Of<IServiceProvider>());
            await Assert.ThrowsAsync<ArgumentException>(() => sut.Publish(key, new FakeEvent("", ""), CancellationToken.None));
        }

        [Fact]
        public async Task ProduceAsyncShouldBeThrowArgumentNullExceptionIfEventIsNull()
        {
            var sut = new EventBus(Mock.Of<IDispatcherManager>(), Mock.Of<IEventTypeCache>(), Mock.Of<IServiceProvider>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.Publish<FakeEvent>(Guid.NewGuid().ToString(), null, CancellationToken.None));
        }

        [Fact]
        public async Task ProduceAsyncShouldGetProduceInformationsGetProducesAndProduce()
        {
            (BusName Bus, Enum Switch, string Endpoint) expectedPublishParameters = (BusName.Kafka, ServerName.Default, FakeEvent.TOPIC_NAME);
            var expectedKey = Guid.NewGuid().ToString();
            var expectedEvent = new FakeEvent("test", "test");

            var moqBusEventProducer = new Mock<IBusEventDispatcher>();

            var moqProducerManager = new Mock<IDispatcherManager>();
            moqProducerManager
                .Setup(x => x.GetDispatcher(BusName.Kafka, It.IsAny<IServiceProvider>()))
                .Returns(() => moqBusEventProducer.Object);

            var moqeventTypeCache = new Mock<IEventTypeCache>();
            moqeventTypeCache
                .Setup(x => x.Get(It.IsAny<Type>()))
                .Returns(new List<(BusName BusName, Enum Switch, string Endpoint)> { expectedPublishParameters });

            var sut = new EventBus(moqProducerManager.Object, moqeventTypeCache.Object, Mock.Of<IServiceProvider>());

            await sut.Publish(expectedKey, expectedEvent, CancellationToken.None);

            moqeventTypeCache
                .Verify(x => x.Get(typeof(FakeEvent)), Times.Once);

            moqProducerManager
                .Verify(x => x.GetDispatcher(expectedPublishParameters.Bus, It.IsAny<IServiceProvider>()), Times.Once());

            moqBusEventProducer
                .Verify(x => x.Dispatch(expectedKey, expectedEvent, expectedPublishParameters.Switch, expectedPublishParameters.Endpoint, CancellationToken.None), Times.Once());
        }
    }
}
