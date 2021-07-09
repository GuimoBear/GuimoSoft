using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using GuimoSoft.MessageBroker.Abstractions;
using GuimoSoft.MessageBroker.Kafka.Common;
using GuimoSoft.MessageBroker.Kafka.Consumer;
using GuimoSoft.MessageBroker.Kafka.Producer;

namespace GuimoSoft.MessageBroker.Kafka
{
    public static class ServiceCollectionExtensions
    {
        public static KafkaServiceCollectionWrapper AddKafkaConsumer(this IServiceCollection services,
            params Type[] handlerAssemblyMarkerTypes)
        {
            services.TryAddSingleton<IKafkaTopicCache>(_ => new KafkaTopicCache(services));

            services.AddMediatR(handlerAssemblyMarkerTypes);

            services.AddTransient<IKafkaMessageConsumerManager>(serviceProvider =>
                new KafkaMessageConsumerManager(serviceProvider, services));

            services.TryAddTransient<IKafkaConsumerBuilder, KafkaConsumerBuilder>();

            services.AddTransient<IKafkaTopicMessageConsumer, KafkaTopicMessageConsumer>();

            services.AddHostedService<KafkaConsumerMessageHandler>();

            return new KafkaServiceCollectionWrapper(services);
        }

        public static IServiceCollection AddKafkaProducer(this IServiceCollection services)
        {
            services.TryAddSingleton<IKafkaTopicCache>(_ => new KafkaTopicCache(services));

            services.TryAddSingleton<IKafkaProducerBuilder, KafkaProducerBuilder>();

            services.TryAddSingleton<IMessageProducer, KafkaMessageProducer>();

            return services;
        }
    }

    public class KafkaServiceCollectionWrapper : IServiceCollection
    {
        private readonly IServiceCollection _serviceCollection;

        private readonly Lazy<IMessageMiddlereManager> _messageMiddlewareManagerSingleton;

        public ServiceDescriptor this[int index]
        {
            get => _serviceCollection[index];
            set => _serviceCollection[index] = value;
        }

        public KafkaServiceCollectionWrapper(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;

            _messageMiddlewareManagerSingleton = new Lazy<IMessageMiddlereManager>(() => new MessageMiddlereManager(_serviceCollection));

            _serviceCollection.AddSingleton(_ => _messageMiddlewareManagerSingleton.Value);
            _serviceCollection.AddSingleton<IMessageMiddlereExecutorProvider>(prov => prov.GetService(typeof(IMessageMiddlereManager)) as IMessageMiddlereManager);
            _serviceCollection.AddSingleton<IMessageMiddlewareRegister>(prov => prov.GetService(typeof(IMessageMiddlereManager)) as IMessageMiddlereManager);
        }

        /// <summary>
        /// Try add a scoped middleware to be executed before mediatR publish
        /// </summary>
        /// <typeparam name="TMessage">The IMessage implementation type</typeparam>
        /// <typeparam name="TType">The middleware implementation type</typeparam>
        /// <returns></returns>
        public KafkaServiceCollectionWrapper WithMessageMiddleware<TMessage, TType>()
            where TMessage : IMessage
            where TType : class, IMessageMiddleware<TMessage>
        {
            _messageMiddlewareManagerSingleton.Value
                .Register<TMessage, TType>();
            return this;
        }

        /// <summary>
        /// Try add a scoped middleware to be executed before mediatR publish
        /// </summary>
        /// <typeparam name="TMessage">The IMessage implementation type</typeparam>
        /// <typeparam name="TType">The middleware implementation type</typeparam>
        /// <param name="factory">The middleware factory</param>
        /// <returns></returns>
        public KafkaServiceCollectionWrapper WithMessageMiddleware<TMessage, TType>(Func<IServiceProvider, TType> factory)
            where TMessage : IMessage
            where TType : class, IMessageMiddleware<TMessage>
        {
            _messageMiddlewareManagerSingleton.Value
                .Register<TMessage, TType>();
            return this;
        }

        public int Count => _serviceCollection.Count;

        public bool IsReadOnly => _serviceCollection.IsReadOnly;

        public void Add(ServiceDescriptor item)
            => _serviceCollection.Add(item);

        public void Clear()
            => _serviceCollection.Clear();

        public bool Contains(ServiceDescriptor item)
            => _serviceCollection.Contains(item);

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
            => _serviceCollection.CopyTo(array, arrayIndex);

        public IEnumerator<ServiceDescriptor> GetEnumerator()
            => _serviceCollection.GetEnumerator();

        public int IndexOf(ServiceDescriptor item)
            => _serviceCollection.IndexOf(item);

        public void Insert(int index, ServiceDescriptor item)
            => _serviceCollection.Insert(index, item);

        public bool Remove(ServiceDescriptor item)
            => _serviceCollection.Remove(item);

        public void RemoveAt(int index)
            => _serviceCollection.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator()
            => _serviceCollection.GetEnumerator();
    }
}