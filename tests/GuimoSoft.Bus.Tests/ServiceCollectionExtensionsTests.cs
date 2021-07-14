using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Kafka;
using GuimoSoft.Bus.Kafka.Common;
using GuimoSoft.Bus.Kafka.Producer;
using GuimoSoft.Bus.Tests.Fakes;
using GuimoSoft.Serialization.Interfaces;
using Xunit;

namespace GuimoSoft.Bus.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void ServiceCollectionExtensionsFacts()
        {
            var services = new ServiceCollection();

            services
                .AddKafkaConsumer(typeof(ServiceCollectionExtensionsTests))
                .WithMessageMiddleware<FakeMessage, FakeMessageMiddleware>()
                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                .WithTypedSerializer(OtherFakeMessageSerializer.Instance);

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IMediator))
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(MediatorPublisherMiddleware<>))
                .Should().NotBeNull();

            var messageMiddlewareServiceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IMessageMiddlewareManager));

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IMessageSerializerManager))
                .Should().NotBeNull();

            messageMiddlewareServiceDescriptor
                .Should().NotBeNull();

            var messageMiddleware = messageMiddlewareServiceDescriptor.ImplementationFactory(null) as MessageMiddlewareManager;

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

            services
                .AddKafkaProducer()
                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                .WithTypedSerializer(OtherFakeMessageSerializer.Instance);

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaTopicCache))
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaProducerBuilder))
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IMessageProducer))
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IMessageSerializerManager))
                .Should().NotBeNull();
        }
    }
}
