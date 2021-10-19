using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Bus.Kafka.Consumer;
using GuimoSoft.Bus.Kafka.Producer;
using GuimoSoft.Bus.Core.Listener;
using GuimoSoft.Bus.Core.Publisher;
using GuimoSoft.Bus.Core.Internal.Middlewares;

namespace GuimoSoft.Bus.Kafka
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddKafkaConsumer(this IServiceCollection services, Action<ListenerBuilder<ConsumerConfig>> configurer)
        {
            AddConsumerDependencies(services);
            using var register = new AssemblyRegister(services);
            var configs = new ListenerBuilder<ConsumerConfig>(BusName.Kafka, ServerName.Default, register.Assemblies, services);
            configurer(configs);
            configs.ValidateAfterConfigured();
            return services;
        }

        public static IServiceCollection AddKafkaConsumerSwitcher<TSwitch>(this IServiceCollection services,
            Action<ListenerSwitcherBuilder<TSwitch, ConsumerConfig>> configurer)
            where TSwitch : struct, Enum
        {
            AddConsumerDependencies(services);
            using var register = new AssemblyRegister(services);
            var configs = new ListenerSwitcherBuilder<TSwitch, ConsumerConfig>(BusName.Kafka, register.Assemblies, services);
            configurer(configs);
            configs.ValidateAfterConfigured();
            return services;
        }

        public static IServiceCollection AddKafkaProducer(this IServiceCollection services, Action<PublisherBuilder<ProducerConfig>> configurer)
        {
            AddProducerDependencies(services);
            using var register = new AssemblyRegister(services);
            var configs = new PublisherBuilder<ProducerConfig>(BusName.Kafka, ServerName.Default, register.Assemblies, services);
            configurer(configs);
            configs.ValidateAfterConfigured();
            return services;
        }

        public static IServiceCollection AddKafkaProducerSwitcher<TSwitch>(this IServiceCollection services,
            Action<PublisherSwitcherBuilder<TSwitch, ProducerConfig>> configurer)
            where TSwitch : struct, Enum
        {
            AddProducerDependencies(services);
            using var register = new AssemblyRegister(services);
            var configs = new PublisherSwitcherBuilder<TSwitch, ProducerConfig>(BusName.Kafka, register.Assemblies, services);
            configurer(configs);
            configs.ValidateAfterConfigured();
            return services;
        }

        private static void AddConsumerDependencies(IServiceCollection services)
        {
            services.TryAddTransient<IKafkaEventConsumerManager, KafkaEventConsumerManager>();

            services.TryAddSingleton(typeof(IConsumeContextAccessor<>), typeof(ConsumeContextAccessor<>));

            services.TryAddSingleton(typeof(IBusLogDispatcher), typeof(BusLogDispatcher));

            services.TryAddSingleton(typeof(EventDispatcherMiddleware<>));

            services.TryAddTransient<IKafkaConsumerBuilder, KafkaConsumerBuilder>();

            services.TryAddTransient<IKafkaTopicEventConsumer, KafkaTopicEventConsumer>();

            services.AddHostedService<KafkaConsumerEventHandler>();
        }

        private static void AddProducerDependencies(IServiceCollection services)
        {
            services.TryAddTransient<IKafkaProducerBuilder, KafkaProducerBuilder>();

            Singletons
                .TryRegisterAndGetDispatcherManager(services)
                .Add<KafkaEventProducer>(BusName.Kafka);
        }

        private sealed class AssemblyRegister : IDisposable
        {
            private readonly IServiceCollection _services;
            public ICollection<Assembly> Assemblies { get; }

            public AssemblyRegister(IServiceCollection services)
            {
                _services = services;
                Assemblies = Singletons.GetAssemblies();
            }

            public void Dispose()
            {
                _services.RegisterMediatorFromNewAssemblies(Assemblies.ToArray());
            }
        }
    }
}