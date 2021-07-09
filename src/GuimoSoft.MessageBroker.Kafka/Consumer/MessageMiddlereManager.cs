using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GuimoSoft.MessageBroker.Abstractions;

namespace GuimoSoft.MessageBroker.Kafka.Consumer
{
    public class MessageMiddlereManager : IMessageMiddlereManager
    {
        private readonly IServiceCollection _serviceCollection;

        internal readonly ConcurrentDictionary<Type, Pipeline> pipelines
            = new ();

        internal readonly ConcurrentDictionary<Type, ConcurrentBag<Type>> messageMiddlewareTypes
            = new ();

        public MessageMiddlereManager(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public Pipeline GetPipeline(Type messageType)
        {
            if (!pipelines.TryGetValue(messageType, out var pipeline))
            {
                var middlewares = new List<Type>();
                if (messageMiddlewareTypes.TryGetValue(messageType, out var middlewaresBag))
                    middlewares.AddRange(middlewaresBag);
                pipeline = new Pipeline(middlewares);
            }
            return pipeline;
        }

        public void Register<TMessage, TType>()
            where TMessage : IMessage
            where TType : class, IMessageMiddleware<TMessage>
        {
            var middlewareType = typeof(TType);
            var messageType = typeof(TMessage);

            if (messageMiddlewareTypes.TryGetValue(messageType, out var middlewares) &&
                !middlewares.ToList().Contains(middlewareType))
            {
                middlewares.Add(middlewareType);
            }
            else
            {
                if (!messageMiddlewareTypes.TryAdd(messageType, new ConcurrentBag<Type> { middlewareType }))
                    throw new InvalidOperationException($"Não foi possível registrar o middleware do tipo '{middlewareType.FullName}'");
            }
            _serviceCollection.TryAddScoped<TType>();
        }

        public void Register<TMessage, TType>(Func<IServiceProvider, TType> factory)
            where TMessage : IMessage
            where TType : class, IMessageMiddleware<TMessage>
        {
            var middlewareType = typeof(TType);
            var messageType = typeof(TMessage);

            if (messageMiddlewareTypes.TryGetValue(messageType, out var middlewares) &&
                !middlewares.ToList().Contains(middlewareType))
            {
                middlewares.Add(middlewareType);
            }
            else
            {
                if (!messageMiddlewareTypes.TryAdd(messageType, new ConcurrentBag<Type> { middlewareType }))
                    throw new InvalidOperationException($"Não foi possível registrar o middleware do tipo '{middlewareType.FullName}'");
            }

            if (factory is not default(Func<IServiceProvider, TType>))
                _serviceCollection.TryAddSingleton(factory);
            else
                _serviceCollection.TryAddSingleton<TType>();
        }
    }
}
