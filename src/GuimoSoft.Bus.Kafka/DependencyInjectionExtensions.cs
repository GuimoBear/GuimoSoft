using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Bus.Core.Logs.Interfaces;
using GuimoSoft.Bus.Kafka.Common;
using GuimoSoft.Bus.Kafka.Consumer;
using GuimoSoft.Bus.Kafka.Producer;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace GuimoSoft.Bus.Kafka
{
    public static class DependencyInjectionExtensions
    {
        public static BusConsumerServiceCollectionWrapper AddKafkaConsumer(this IServiceCollection services,
            params Type[] handlerAssemblyMarkerTypes)
        {
            services.TryAddSingleton<IBusLogger, DefaultBusLogger>();

            services.TryAddSingleton<IKafkaTopicCache>(_ => new KafkaTopicCache(services));

            services.AddMediatR(handlerAssemblyMarkerTypes);

            services.AddTransient<IKafkaMessageConsumerManager>(serviceProvider =>
                new KafkaMessageConsumerManager(serviceProvider, services));

            services.TryAddTransient<IKafkaConsumerBuilder, KafkaConsumerBuilder>();

            services.AddTransient<IKafkaTopicMessageConsumer, KafkaTopicMessageConsumer>();

            services.AddHostedService<KafkaConsumerMessageHandler>();

            return new BusConsumerServiceCollectionWrapper(services);
        }

        public static BusServiceCollectionWrapper AddKafkaProducer(this IServiceCollection services)
        {
            services.TryAddSingleton<IBusLogger, DefaultBusLogger>();

            services.TryAddSingleton<IKafkaTopicCache>(_ => new KafkaTopicCache(services));

            services.TryAddSingleton<IKafkaProducerBuilder, KafkaProducerBuilder>();

            services.TryAddSingleton<IMessageProducer, KafkaMessageProducer>();

            return new BusServiceCollectionWrapper(services);
        }
    }
}