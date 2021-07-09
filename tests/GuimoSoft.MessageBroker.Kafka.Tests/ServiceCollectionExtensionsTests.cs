using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using GuimoSoft.MessageBroker.Abstractions;
using GuimoSoft.MessageBroker.Kafka.Common;
using GuimoSoft.MessageBroker.Kafka.Consumer;
using GuimoSoft.MessageBroker.Kafka.Producer;
using GuimoSoft.MessageBroker.Kafka.Tests.Fakes;
using Xunit;

namespace GuimoSoft.MessageBroker.Kafka.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void ServiceCollectionExtensionsFacts()
        {
            var services = new ServiceCollection();

            var kafkaWrapper = services
                .AddKafkaConsumer(typeof(ServiceCollectionExtensionsTests))
                .WithMessageMiddleware<FakeMessage, FakeMessageMiddleware>();

            kafkaWrapper.FirstOrDefault(sd => sd.ServiceType == typeof(IMediator))
                .Should().NotBeNull();

            var messageMiddlewareServiceDescriptor = kafkaWrapper.FirstOrDefault(sd => sd.ServiceType == typeof(IMessageMiddlereManager));

            messageMiddlewareServiceDescriptor
                .Should().NotBeNull();

            var messageMiddleware = messageMiddlewareServiceDescriptor.ImplementationFactory(null) as MessageMiddlereManager;

            messageMiddleware
                .Should().NotBeNull();

            messageMiddleware
                .messageMiddlewareTypes
                .Should().ContainKey(typeof(FakeMessage));

            messageMiddleware
                .messageMiddlewareTypes[typeof(FakeMessage)]
                .Should().NotBeNullOrEmpty();


            messageMiddleware
                .messageMiddlewareTypes[typeof(FakeMessage)]
                .First()
                .Should().Be(typeof(FakeMessageMiddleware));

            services = new ServiceCollection();

            services.AddKafkaProducer();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaTopicCache))
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaProducerBuilder))
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IMessageProducer))
                .Should().NotBeNull();
        }
    }
}
