using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Kafka.Producer;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Internal
{
    public class ProducerManagerTests
    {
        [Fact]
        public void ConstructorShouldThrowIfAnyParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DispatcherManager(null));
        }

        [Fact]
        public void AddFacts()
        {
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IKafkaProducerBuilder>());
            services.AddSingleton(Mock.Of<IBusSerializerManager>());

            var sut = new DispatcherManager(services);

            sut.Add<KafkaEventProducer>(BusName.Kafka);

            services
                .FirstOrDefault(sd => sd.ServiceType == typeof(KafkaEventProducer))
                .Should().NotBeNull();

            sut.Add<FakeEventProducer>(BusName.Kafka);

            services
                .FirstOrDefault(sd => sd.ServiceType == typeof(KafkaEventProducer))
                .Should().NotBeNull();

            services
                .FirstOrDefault(sd => sd.ServiceType == typeof(FakeEventProducer))
                .Should().BeNull();
        }

        [Fact]
        public void GetProducerFacts()
        {
            var services = new ServiceCollection();

            var sut = new DispatcherManager(services);

            Assert.Throws<InvalidOperationException>(() => sut.GetDispatcher(BusName.Kafka, Mock.Of<IServiceProvider>()));

            sut.Add<FakeEventProducer>(BusName.Kafka);

            var moqServiceProvider = new Mock<IServiceProvider>();
            moqServiceProvider
                .Setup(x => x.GetService(typeof(FakeEventProducer)))
                .Returns(FakeEventProducer.Instance);

            var producer = sut.GetDispatcher(BusName.Kafka, moqServiceProvider.Object);

            producer
                .Should().BeSameAs(FakeEventProducer.Instance);
        }

    }
}
