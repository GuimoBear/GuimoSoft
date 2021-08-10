using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Interfaces;

namespace GuimoSoft.Bus.Core.Internal
{
    internal class MessageMiddlewareManager : IMessageMiddlewareManager
    {
        private readonly IServiceCollection _serviceCollection;

        internal readonly ConcurrentDictionary<(BusName, Enum, Type), Pipeline> pipelines
            = new();

        internal readonly ConcurrentDictionary<(BusName, Enum, Type), ConcurrentBag<Type>> messageMiddlewareTypes
            = new();

        public MessageMiddlewareManager(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public Pipeline GetPipeline(BusName brokerName, Enum @switch, Type messageType)
        {
            if (!pipelines.TryGetValue((brokerName, @switch, messageType), out var pipeline))
            {
                var middlewares = new List<Type>();
                middlewares.Add(typeof(ConsumeContextAccessorInitializerMiddleware<>).MakeGenericType(messageType));
                if (messageMiddlewareTypes.TryGetValue((brokerName, @switch, messageType), out var middlewaresBag))
                    middlewares.AddRange(middlewaresBag);
                middlewares.Add(typeof(MediatorPublisherMiddleware<>).MakeGenericType(messageType));
                pipeline = new Pipeline(middlewares, messageType);
                pipelines.TryAdd((brokerName, @switch, messageType), pipeline);
            }
            return pipeline;
        }

        public void Register<TMessage, TMiddleware>(BusName brokerName, Enum @switch, ServiceLifetime lifetime)
            where TMessage : IMessage
            where TMiddleware : class, IMessageMiddleware<TMessage>
        {
            Register<TMessage, TMiddleware>(brokerName, @switch, default, lifetime);
        }

        public void Register<TMessage, TMiddleware>(BusName brokerName, Enum @switch, Func<IServiceProvider, TMiddleware> factory, ServiceLifetime lifetime)
            where TMessage : IMessage
            where TMiddleware : class, IMessageMiddleware<TMessage>
        {
            AddMiddlewareType<TMessage, TMiddleware>(brokerName, @switch);

            if (factory is not default(Func<IServiceProvider, TMiddleware>))
                _serviceCollection.TryAdd(ServiceDescriptor.Describe(typeof(TMiddleware), factory, lifetime));
            else
                _serviceCollection.TryAdd(ServiceDescriptor.Describe(typeof(TMiddleware), typeof(TMiddleware), lifetime));
        }

        private void AddMiddlewareType<TMessage, TMiddleware>(BusName brokerName, Enum @switch)
            where TMessage : IMessage
            where TMiddleware : class, IMessageMiddleware<TMessage>
        {
            var middlewareType = typeof(TMiddleware);
            var messageType = typeof(TMessage);

            if (messageMiddlewareTypes.TryGetValue((brokerName, @switch, messageType), out var middlewares) &&
                !middlewares.ToList().Contains(middlewareType))
            {
                middlewares.Add(middlewareType);
            }
            else
            {
                if (!messageMiddlewareTypes.TryAdd((brokerName, @switch, messageType), new ConcurrentBag<Type> { middlewareType }))
                    throw new InvalidOperationException($"Não foi possível registrar o middleware do tipo '{middlewareType.FullName}'");
            }
        }
    }
}
