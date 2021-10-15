using Confluent.Kafka;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Exceptions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Kafka;
using GuimoSoft.Bus.Kafka.Consumer;
using GuimoSoft.Bus.Kafka.Producer;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void ServiceCollectionExtensionsWithDefaultConfigFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var services = new ServiceCollection();

                services
                    .AddKafkaConsumer(configurer =>
                    {
                        configurer
                            .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                            .Listen()
                                .OfType<FakeEvent>()
                                .WithMiddleware<FakeEventMiddleware>(ServiceLifetime.Transient)
                                .FromEndpoint(FakeEvent.TOPIC_NAME)
                            .Listen()
                                .OfType<OtherFakeEvent>()
                                .WithSerializer(OtherFakeEventSerializer.Instance)
                                .FromEndpoint(OtherFakeEvent.TOPIC_NAME)
                            .Listen()
                                .OfType<FakePipelineEvent>()
                                .WithMiddleware<FakePipelineEventMiddlewareOne>(ServiceLifetime.Scoped)
                                .WithMiddleware(_ => new FakePipelineEventMiddlewareTwo(), ServiceLifetime.Singleton)
                                .FromEndpoint(FakePipelineEvent.TOPIC_NAME)
                            .FromServer(options =>
                            {
                                options.GroupId = "test";
                                options.BootstrapServers = "localhost";
                            })
                            .AddAnotherAssembliesToMediatR(typeof(ServiceCollectionExtensionsTests).Assembly);
                    })
                    .AddKafkaProducer(configurer =>
                    {
                        configurer
                            .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                            .Publish()
                                .OfType<OtherFakeEvent>()
                                .WithSerializer(OtherFakeEventSerializer.Instance)
                                .ToEndpoint(OtherFakeEvent.TOPIC_NAME)
                            .Publish()
                                .OfType<OtherFakeEvent>()
                                .WithSerializer(OtherFakeEventSerializer.Instance)
                                .ToEndpoint(OtherFakeEvent.TOPIC_NAME)
                            .ToServer(options =>
                            {
                                options.BootstrapServers = "localhost";
                                options.Acks = Acks.All;
                            })
                            .AddAnotherAssembliesToMediatR(typeof(ServiceCollectionExtensionsTests).Assembly);
                    });

                services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaEventConsumerManager)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IConsumeContextAccessor<>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusLogDispatcher)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(ConsumeContextAccessorInitializerMiddleware<>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(MediatorPublisherMiddleware<>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaConsumerBuilder)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaTopicEventConsumer)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ImplementationType == typeof(KafkaConsumerEventHandler)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusSerializerManager)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventMiddlewareExecutorProvider)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventMiddlewareRegister)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventTypeCache)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusOptionsDictionary<ConsumerConfig>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusOptionsDictionary<ProducerConfig>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IMediator)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaProducerBuilder)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventBus)).Should().NotBeNull();

                var eventMiddlewareServiceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventMiddlewareManager));

                eventMiddlewareServiceDescriptor
                    .Should().NotBeNull();

                var eventMiddleware = eventMiddlewareServiceDescriptor.ImplementationInstance as EventMiddlewareManager;

                eventMiddleware
                    .Should().NotBeNull();

                eventMiddleware
                    .eventMiddlewareTypes
                    .Should().ContainKey((BusName.Kafka, Bus.Abstractions.ServerName.Default, typeof(FakeEvent)));

                eventMiddleware
                    .eventMiddlewareTypes[(BusName.Kafka, Bus.Abstractions.ServerName.Default, typeof(FakeEvent))]
                    .Should().NotBeNullOrEmpty();

                eventMiddleware
                    .eventMiddlewareTypes[(BusName.Kafka, Bus.Abstractions.ServerName.Default, typeof(FakeEvent))]
                    .First()
                    .Should().Be(typeof(FakeEventMiddleware));

                var sp = services.BuildServiceProvider(true);

                sp.GetRequiredService<IEventBus>();
                sp.GetRequiredService<IKafkaTopicEventConsumer>();
            }
        }

        [Fact]
        public void ServiceCollectionExtensionsWithSwitcheableConfigFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var services = new ServiceCollection();

                services
                    .AddKafkaConsumerSwitcher<ServerName>(switcher =>
                    {
                        switcher
                            .AddAnotherAssembliesToMediatR(typeof(ServiceCollectionExtensionsTests).Assembly);

                        switcher
                            .When(ServerName.Host1)
                                .FromServer(options =>
                                {
                                    options.GroupId = "test";
                                    options.BootstrapServers = "localhost";
                                })
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Listen()
                                    .OfType<FakeEvent>()
                                    .WithMiddleware(_ => new FakeEventMiddleware())
                                    .FromEndpoint(FakeEvent.TOPIC_NAME)
                                .Listen()
                                    .OfType<OtherFakeEvent>()
                                    .WithSerializer(OtherFakeEventSerializer.Instance)
                                    .FromEndpoint(OtherFakeEvent.TOPIC_NAME)
                                .Listen()
                                    .OfType<FakePipelineEvent>()
                                    .WithMiddleware<FakePipelineEventMiddlewareOne>()
                                    .WithMiddleware(_ => new FakePipelineEventMiddlewareTwo())
                                    .FromEndpoint(FakePipelineEvent.TOPIC_NAME);

                        switcher
                            .When(ServerName.Host2)
                                .FromServer(options =>
                                {
                                    options.GroupId = "test";
                                    options.BootstrapServers = "localhost";
                                })
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Listen()
                                    .OfType<FakeEvent>()
                                    .WithMiddleware<FakeEventMiddleware>(ServiceLifetime.Transient)
                                    .FromEndpoint(FakeEvent.TOPIC_NAME)
                                .Listen()
                                    .OfType<OtherFakeEvent>()
                                    .WithSerializer(OtherFakeEventSerializer.Instance)
                                    .FromEndpoint(OtherFakeEvent.TOPIC_NAME)
                                .Listen()
                                    .OfType<FakePipelineEvent>()
                                    .WithMiddleware<FakePipelineEventMiddlewareOne>()
                                    .WithMiddleware(_ => new FakePipelineEventMiddlewareTwo())
                                    .FromEndpoint(FakePipelineEvent.TOPIC_NAME);

                    })
                    .AddKafkaProducerSwitcher<ServerName>(switcher =>
                    {
                        switcher
                            .AddAnotherAssembliesToMediatR(typeof(ServiceCollectionExtensionsTests).Assembly);

                        switcher
                            .When(ServerName.Host1)
                                .ToServer(options =>
                                {
                                    options.BootstrapServers = "localhost";
                                    options.Acks = Acks.All;
                                })
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Publish()
                                    .OfType<FakeEvent>()
                                    .ToEndpoint(FakeEvent.TOPIC_NAME)
                                .Publish()
                                    .OfType<OtherFakeEvent>()
                                    .WithSerializer(OtherFakeEventSerializer.Instance)
                                    .ToEndpoint(OtherFakeEvent.TOPIC_NAME);

                        switcher
                            .When(ServerName.Host2)
                                .ToServer(options =>
                                {
                                    options.BootstrapServers = "localhost";
                                    options.Acks = Acks.All;
                                })
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Publish()
                                    .OfType<FakeEvent>()
                                    .ToEndpoint(FakeEvent.TOPIC_NAME)
                                .Publish()
                                    .OfType<OtherFakeEvent>()
                                    .WithSerializer(OtherFakeEventSerializer.Instance)
                                    .ToEndpoint(OtherFakeEvent.TOPIC_NAME);
                    });

                services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaEventConsumerManager)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IConsumeContextAccessor<>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusLogDispatcher)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(ConsumeContextAccessorInitializerMiddleware<>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(MediatorPublisherMiddleware<>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaConsumerBuilder)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaTopicEventConsumer)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ImplementationType == typeof(KafkaConsumerEventHandler)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusSerializerManager)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventMiddlewareExecutorProvider)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventMiddlewareRegister)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventTypeCache)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusOptionsDictionary<ConsumerConfig>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusOptionsDictionary<ProducerConfig>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IMediator)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaProducerBuilder)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventBus)).Should().NotBeNull();

                var eventMiddlewareServiceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventMiddlewareManager));

                eventMiddlewareServiceDescriptor
                    .Should().NotBeNull();

                var eventMiddleware = eventMiddlewareServiceDescriptor.ImplementationInstance as EventMiddlewareManager;

                eventMiddleware
                    .Should().NotBeNull();

                eventMiddleware
                    .eventMiddlewareTypes
                    .Should().ContainKey((BusName.Kafka, ServerName.Host1, typeof(FakeEvent)));

                eventMiddleware
                    .eventMiddlewareTypes
                    .Should().ContainKey((BusName.Kafka, ServerName.Host2, typeof(FakeEvent)));

                eventMiddleware
                    .eventMiddlewareTypes[(BusName.Kafka, ServerName.Host1, typeof(FakeEvent))]
                    .Should().NotBeNullOrEmpty();

                eventMiddleware
                    .eventMiddlewareTypes[(BusName.Kafka, ServerName.Host2, typeof(FakeEvent))]
                    .Should().NotBeNullOrEmpty();

                eventMiddleware
                    .eventMiddlewareTypes[(BusName.Kafka, ServerName.Host1, typeof(FakeEvent))]
                    .First()
                    .Should().Be(typeof(FakeEventMiddleware));

                eventMiddleware
                    .eventMiddlewareTypes[(BusName.Kafka, ServerName.Host2, typeof(FakeEvent))]
                    .First()
                    .Should().Be(typeof(FakeEventMiddleware));

                var sp = services.BuildServiceProvider(true);

                sp.GetRequiredService<IEventBus>();
                sp.GetRequiredService<IKafkaTopicEventConsumer>();
            }
        }

        [Fact]
        public void BusOptionsMissingOnConsumerFacts()
        {
            lock (Utils.Lock)
            {
                var services = new ServiceCollection();

                Utils.ResetarSingletons();
                Assert.Throws<BusOptionsMissingException>(() =>
                {
                    services.AddKafkaConsumer(configurer =>
                    {
                        configurer
                            .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                            .Listen()
                                .OfType<FakeEvent>()
                                .WithMiddleware<FakeEventMiddleware>(ServiceLifetime.Transient)
                                .FromEndpoint(FakeEvent.TOPIC_NAME)
                            .Listen()
                                .OfType<OtherFakeEvent>()
                                .WithSerializer(OtherFakeEventSerializer.Instance)
                                .FromEndpoint(OtherFakeEvent.TOPIC_NAME)
                            .Listen()
                                .OfType<FakePipelineEvent>()
                                .WithMiddleware<FakePipelineEventMiddlewareOne>(ServiceLifetime.Scoped)
                                .WithMiddleware(_ => new FakePipelineEventMiddlewareTwo(), ServiceLifetime.Singleton)
                                .FromEndpoint(FakePipelineEvent.TOPIC_NAME);
                    });
                });
                Utils.ResetarSingletons();

                Assert.Throws<BusOptionsMissingException>(() =>
                {
                    services.AddKafkaConsumerSwitcher<ServerName>(switcher =>
                    {
                        switcher
                            .When(ServerName.Host1)
                                .FromServer(options =>
                                {
                                    options.GroupId = "test";
                                    options.BootstrapServers = "localhost";
                                })
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Listen()
                                    .OfType<FakeEvent>()
                                    .WithMiddleware(_ => new FakeEventMiddleware())
                                    .FromEndpoint(FakeEvent.TOPIC_NAME)
                                .Listen()
                                    .OfType<OtherFakeEvent>()
                                    .WithSerializer(OtherFakeEventSerializer.Instance)
                                    .FromEndpoint(OtherFakeEvent.TOPIC_NAME)
                                .Listen()
                                    .OfType<FakePipelineEvent>()
                                    .WithMiddleware<FakePipelineEventMiddlewareOne>()
                                    .WithMiddleware(_ => new FakePipelineEventMiddlewareTwo())
                                    .FromEndpoint(FakePipelineEvent.TOPIC_NAME);

                        switcher
                            .When(ServerName.Host2)
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Listen()
                                    .OfType<FakeEvent>()
                                    .WithMiddleware<FakeEventMiddleware>(ServiceLifetime.Transient)
                                    .FromEndpoint(FakeEvent.TOPIC_NAME)
                                .Listen()
                                    .OfType<OtherFakeEvent>()
                                    .WithSerializer(OtherFakeEventSerializer.Instance)
                                    .FromEndpoint(OtherFakeEvent.TOPIC_NAME)
                                .Listen()
                                    .OfType<FakePipelineEvent>()
                                    .WithMiddleware<FakePipelineEventMiddlewareOne>()
                                    .WithMiddleware(_ => new FakePipelineEventMiddlewareTwo())
                                    .FromEndpoint(FakePipelineEvent.TOPIC_NAME);
                    });
                });
                Utils.ResetarSingletons();
            }
        }

        [Fact]
        public void BusOptionsMissingOnProducerFacts()
        {
            lock (Utils.Lock)
            {
                var services = new ServiceCollection();

                Utils.ResetarSingletons();
                Assert.Throws<BusOptionsMissingException>(() =>
                {
                    services.AddKafkaProducer(configurer =>
                    {
                        configurer
                            .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                            .Publish()
                                .OfType<OtherFakeEvent>()
                                .WithSerializer(OtherFakeEventSerializer.Instance)
                                .ToEndpoint(OtherFakeEvent.TOPIC_NAME)
                            .Publish()
                                .OfType<OtherFakeEvent>()
                                .WithSerializer(OtherFakeEventSerializer.Instance)
                                .ToEndpoint(OtherFakeEvent.TOPIC_NAME);
                    });
                });
                Utils.ResetarSingletons();

                Assert.Throws<BusOptionsMissingException>(() =>
                {
                    services.AddKafkaProducerSwitcher<ServerName>(switcher =>
                    {
                        switcher
                            .When(ServerName.Host1)
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Publish()
                                    .OfType<FakeEvent>()
                                    .ToEndpoint(FakeEvent.TOPIC_NAME)
                                .Publish()
                                    .OfType<OtherFakeEvent>()
                                    .WithSerializer(OtherFakeEventSerializer.Instance)
                                    .ToEndpoint(OtherFakeEvent.TOPIC_NAME);

                        switcher
                            .When(ServerName.Host2)
                                .ToServer(options =>
                                {
                                    options.BootstrapServers = "localhost";
                                    options.Acks = Acks.All;
                                })
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Publish()
                                    .OfType<FakeEvent>()
                                    .ToEndpoint(FakeEvent.TOPIC_NAME)
                                .Publish()
                                    .OfType<OtherFakeEvent>()
                                    .WithSerializer(OtherFakeEventSerializer.Instance)
                                    .ToEndpoint(OtherFakeEvent.TOPIC_NAME);
                    });
                });
                Utils.ResetarSingletons();
            }
        }

        [Fact]
        public void BusAlreadyConfiguredExceptionOnConsumerFacts()
        {
            lock (Utils.Lock)
            {
                var services = new ServiceCollection();

                Utils.ResetarSingletons();
                Assert.Throws<BusAlreadyConfiguredException>(() =>
                {
                    services.AddKafkaConsumer(configurer =>
                    {
                        configurer
                            .FromServer(options =>
                            {
                                options.GroupId = "test";
                                options.BootstrapServers = "localhost";
                            })
                            .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                            .Listen()
                                .OfType<FakeEvent>()
                                .WithMiddleware<FakeEventMiddleware>(ServiceLifetime.Transient)
                                .FromEndpoint(FakeEvent.TOPIC_NAME)
                            .Listen()
                                .OfType<OtherFakeEvent>()
                                .WithSerializer(OtherFakeEventSerializer.Instance)
                                .FromEndpoint(OtherFakeEvent.TOPIC_NAME)
                            .Listen()
                                .OfType<FakePipelineEvent>()
                                .WithMiddleware<FakePipelineEventMiddlewareOne>(ServiceLifetime.Scoped)
                                .WithMiddleware(_ => new FakePipelineEventMiddlewareTwo(), ServiceLifetime.Singleton)
                                .FromEndpoint(FakePipelineEvent.TOPIC_NAME);
                    });

                    services.AddKafkaConsumer(configurer =>
                    {
                        configurer
                            .FromServer(options =>
                            {
                                options.GroupId = "test";
                                options.BootstrapServers = "localhost";
                            });
                    });
                });
                Utils.ResetarSingletons();

                Assert.Throws<BusAlreadyConfiguredException>(() =>
                {
                    services.AddKafkaConsumerSwitcher<ServerName>(switcher =>
                    {
                        switcher
                            .When(ServerName.Host1)
                                .FromServer(options =>
                                {
                                    options.GroupId = "test";
                                    options.BootstrapServers = "localhost";
                                })
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Listen()
                                    .OfType<FakeEvent>()
                                    .WithMiddleware(_ => new FakeEventMiddleware())
                                    .FromEndpoint(FakeEvent.TOPIC_NAME)
                                .Listen()
                                    .OfType<OtherFakeEvent>()
                                    .WithSerializer(OtherFakeEventSerializer.Instance)
                                    .FromEndpoint(OtherFakeEvent.TOPIC_NAME)
                                .Listen()
                                    .OfType<FakePipelineEvent>()
                                    .WithMiddleware<FakePipelineEventMiddlewareOne>()
                                    .WithMiddleware(_ => new FakePipelineEventMiddlewareTwo())
                                    .FromEndpoint(FakePipelineEvent.TOPIC_NAME);

                        switcher
                            .When(ServerName.Host2)
                                .FromServer(options =>
                                {
                                    options.GroupId = "test";
                                    options.BootstrapServers = "localhost";
                                })
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Listen()
                                    .OfType<FakeEvent>()
                                    .WithMiddleware<FakeEventMiddleware>(ServiceLifetime.Transient)
                                    .FromEndpoint(FakeEvent.TOPIC_NAME)
                                .Listen()
                                    .OfType<OtherFakeEvent>()
                                    .WithSerializer(OtherFakeEventSerializer.Instance)
                                    .FromEndpoint(OtherFakeEvent.TOPIC_NAME)
                                .Listen()
                                    .OfType<FakePipelineEvent>()
                                    .WithMiddleware<FakePipelineEventMiddlewareOne>()
                                    .WithMiddleware(_ => new FakePipelineEventMiddlewareTwo())
                                    .FromEndpoint(FakePipelineEvent.TOPIC_NAME);
                    });

                    services.AddKafkaConsumerSwitcher<ServerName>(switcher =>
                    {
                        switcher
                            .When(ServerName.Host1)
                                .FromServer(options =>
                                {
                                    options.GroupId = "test";
                                    options.BootstrapServers = "localhost";
                                });

                        switcher
                            .When(ServerName.Host2)
                                .FromServer(options =>
                                {
                                    options.GroupId = "test";
                                    options.BootstrapServers = "localhost";
                                });
                    });
                });

                services.AddKafkaConsumerSwitcher<ServerName>(switcher =>
                {
                    switcher
                        .When(ServerName.Host3)
                            .Listen()
                                .OfType<FakeEvent>()
                                .WithMiddleware<FakeEventMiddleware>(ServiceLifetime.Transient)
                                .FromEndpoint(FakeEvent.TOPIC_NAME)
                            .FromServer(options =>
                            {
                                options.GroupId = "test";
                                options.BootstrapServers = "localhost";
                            });
                });

                Utils.ResetarSingletons();
            }
        }

        [Fact]
        public void BusAlreadyConfiguredExceptionOnProducerFacts()
        {
            lock (Utils.Lock)
            {
                var services = new ServiceCollection();

                Utils.ResetarSingletons();
                Assert.Throws<BusAlreadyConfiguredException>(() =>
                {
                    services.AddKafkaProducer(configurer =>
                    {
                        configurer
                            .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                            .Publish()
                                .OfType<OtherFakeEvent>()
                                .WithSerializer(OtherFakeEventSerializer.Instance)
                                .ToEndpoint(OtherFakeEvent.TOPIC_NAME)
                            .Publish()
                                .OfType<OtherFakeEvent>()
                                .WithSerializer(OtherFakeEventSerializer.Instance)
                                .ToEndpoint(OtherFakeEvent.TOPIC_NAME)
                            .ToServer(options =>
                            {
                                options.BootstrapServers = "localhost";
                                options.Acks = Acks.All;
                            });
                    });

                    services.AddKafkaProducer(configurer =>
                    {
                        configurer
                            .ToServer(options =>
                            {
                                options.BootstrapServers = "localhost";
                                options.Acks = Acks.All;
                            });
                    });
                });
                Utils.ResetarSingletons();

                Assert.Throws<BusAlreadyConfiguredException>(() =>
                {
                    services.AddKafkaProducerSwitcher<ServerName>(switcher =>
                    {
                        switcher
                            .When(ServerName.Host1)
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Publish()
                                    .OfType<FakeEvent>()
                                    .ToEndpoint(FakeEvent.TOPIC_NAME)
                                .Publish()
                                    .OfType<OtherFakeEvent>()
                                    .WithSerializer(OtherFakeEventSerializer.Instance)
                                    .ToEndpoint(OtherFakeEvent.TOPIC_NAME)
                                .ToServer(options =>
                                {
                                    options.BootstrapServers = "localhost";
                                    options.Acks = Acks.All;
                                });

                        switcher
                            .When(ServerName.Host2)
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Publish()
                                    .OfType<FakeEvent>()
                                    .ToEndpoint(FakeEvent.TOPIC_NAME)
                                .Publish()
                                    .OfType<OtherFakeEvent>()
                                    .WithSerializer(OtherFakeEventSerializer.Instance)
                                    .ToEndpoint(OtherFakeEvent.TOPIC_NAME)
                                .ToServer(options =>
                                {
                                    options.BootstrapServers = "localhost";
                                    options.Acks = Acks.All;
                                });
                    });

                    services.AddKafkaProducerSwitcher<ServerName>(switcher =>
                    {
                        switcher
                            .When(ServerName.Host1)
                                .ToServer(options =>
                                {
                                    options.BootstrapServers = "localhost";
                                    options.Acks = Acks.All;
                                });

                        switcher
                            .When(ServerName.Host2)
                                .ToServer(options =>
                                {
                                    options.BootstrapServers = "localhost";
                                    options.Acks = Acks.All;
                                });
                    });
                });

                services.AddKafkaProducerSwitcher<ServerName>(switcher =>
                {
                    switcher
                        .When(ServerName.Host3)
                            .Publish()
                                .OfType<FakeEvent>()
                                .ToEndpoint(FakeEvent.TOPIC_NAME)
                            .ToServer(options =>
                            {
                                options.BootstrapServers = "localhost";
                                options.Acks = Acks.All;
                            });
                });
                Utils.ResetarSingletons();
            }
        }

        private enum ServerName
        {
            Host1,
            Host2, 
            Host3
        }
    }
}
