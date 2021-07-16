using FluentAssertions;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Bus.Core.Logs.Interfaces;
using GuimoSoft.Bus.Kafka;
using GuimoSoft.Bus.Kafka.Common;
using GuimoSoft.Bus.Kafka.Producer;
using GuimoSoft.Bus.Tests.Fakes;
using GuimoSoft.Core.Serialization.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Linq;
using Xunit;

namespace GuimoSoft.Bus.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void ServiceCollectionExtensionsFacts()
        {
            var services = new ServiceCollection();

            var wrapper = services
                .AddKafkaConsumer(typeof(ServiceCollectionExtensionsTests))
                .WithMessageMiddleware<FakeMessage, FakeMessageMiddleware>()
                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                .WithTypedSerializer(OtherFakeMessageSerializer.Instance);

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusLogger) &&
                                          sd.ImplementationType == typeof(DefaultBusLogger))
                .Should().NotBeNull();

            Assert.Throws<ArgumentNullException>(() => wrapper.WithLogger((IBusLogger)null));
            Assert.Throws<ArgumentNullException>(() => wrapper.WithLogger((Func<IServiceProvider, IBusLogger>)null));

            var fakeLoggerInstance = new FakeBusLogger();

            wrapper
                .WithLogger(fakeLoggerInstance);

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusLogger) &&
                                          ReferenceEquals(sd.ImplementationInstance, fakeLoggerInstance))
                .Should().NotBeNull();

            Func<IServiceProvider, IBusLogger> fakeLoggerParameterlessFactory = _ => new FakeBusLogger();

            wrapper
                .WithLogger(fakeLoggerParameterlessFactory);

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusLogger) &&
                                          ReferenceEquals(sd.ImplementationFactory, fakeLoggerParameterlessFactory))
                .Should().NotBeNull();

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

            wrapper.Contains(messageMiddlewareServiceDescriptor);
            wrapper.IndexOf(messageMiddlewareServiceDescriptor);
            wrapper.Remove(messageMiddlewareServiceDescriptor);
            wrapper.Insert(0, messageMiddlewareServiceDescriptor);
            wrapper.RemoveAt(0);
            wrapper.Add(messageMiddlewareServiceDescriptor);

            wrapper.GetEnumerator();

            (wrapper as IEnumerable).GetEnumerator();


        }
    }
}
