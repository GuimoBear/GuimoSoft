using Microsoft.Extensions.DependencyInjection;
using System;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Serialization;
using GuimoSoft.Serialization.Interfaces;

namespace GuimoSoft.Bus.Core
{
    public class BusConsumerServiceCollectionWrapper : BusServiceCollectionWrapper
    {
        private readonly Lazy<IMessageMiddlewareManager> _messageMiddlewareManagerSingleton;

        public BusConsumerServiceCollectionWrapper(IServiceCollection serviceCollection) : base(serviceCollection)
        {
            _messageMiddlewareManagerSingleton = new Lazy<IMessageMiddlewareManager>(() => new MessageMiddlewareManager(_serviceCollection));

            _serviceCollection.AddSingleton(_ => _messageMiddlewareManagerSingleton.Value);
            _serviceCollection.AddSingleton<IMessageMiddlewareExecutorProvider>(prov => prov.GetService(typeof(IMessageMiddlewareManager)) as IMessageMiddlewareManager);
            _serviceCollection.AddSingleton<IMessageMiddlewareRegister>(prov => prov.GetService(typeof(IMessageMiddlewareManager)) as IMessageMiddlewareManager);
            _serviceCollection.AddSingleton(typeof(MediatorPublisherMiddleware<>));
        }

        /// <summary>
        /// Try add a scoped middleware to be executed before mediatR publish
        /// </summary>
        /// <typeparam name="TMessage">The IMessage implementation type</typeparam>
        /// <typeparam name="TType">The middleware implementation type</typeparam>
        /// <returns></returns>
        public BusConsumerServiceCollectionWrapper WithMessageMiddleware<TMessage, TType>()
            where TMessage : IMessage
            where TType : class, IMessageMiddleware<TMessage>
        {
            _serviceCollection.AddSingleton<TType>();
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
        public BusConsumerServiceCollectionWrapper WithMessageMiddleware<TMessage, TType>(Func<IServiceProvider, TType> factory)
            where TMessage : IMessage
            where TType : class, IMessageMiddleware<TMessage>
        {
            _messageMiddlewareManagerSingleton.Value
                .Register<TMessage, TType>(factory);
            return this;
        }

        public override BusConsumerServiceCollectionWrapper WithDefaultSerializer(IDefaultSerializer defaultSerializer)
        {
            base.WithDefaultSerializer(defaultSerializer);
            return this;
        }

        public override BusConsumerServiceCollectionWrapper WithTypedSerializer<TMessage>(TypedSerializer<TMessage> serializer)
        {
            base.WithTypedSerializer(serializer);
            return this;
        }
    }
}
