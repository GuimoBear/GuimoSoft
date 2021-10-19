using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GuimoSoft.Bus.Core.Internal
{
    internal class EventMiddlewareManager : IEventMiddlewareManager
    {
        private readonly IServiceCollection _serviceCollection;

        internal readonly ConcurrentDictionary<(BusName, Enum, Type), Pipeline> pipelines
            = new();

        internal readonly ConcurrentDictionary<(BusName, Enum, Type), ConcurrentBag<Type>> eventMiddlewareTypes
            = new();

        public EventMiddlewareManager(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public Pipeline GetPipeline(BusName brokerName, Enum @switch, Type eventType)
        {
            if (!pipelines.TryGetValue((brokerName, @switch, eventType), out var pipeline))
            {
                var middlewares = new List<Type>();
                if (eventMiddlewareTypes.TryGetValue((brokerName, @switch, eventType), out var middlewaresBag))
                    middlewares.AddRange(middlewaresBag);
                middlewares.Add(typeof(EventDispatcherMiddleware<>).MakeGenericType(eventType));
                pipeline = new Pipeline(middlewares, eventType);
                pipelines.TryAdd((brokerName, @switch, eventType), pipeline);
            }
            return pipeline;
        }

        public void Register<TEvent, TMiddleware>(BusName brokerName, Enum @switch, ServiceLifetime lifetime)
            where TEvent : IEvent
            where TMiddleware : class, IEventMiddleware<TEvent>
        {
            Register<TEvent, TMiddleware>(brokerName, @switch, default, lifetime);
        }

        public void Register<TEvent, TMiddleware>(BusName brokerName, Enum @switch, Func<IServiceProvider, TMiddleware> factory, ServiceLifetime lifetime)
            where TEvent : IEvent
            where TMiddleware : class, IEventMiddleware<TEvent>
        {
            AddMiddlewareType<TEvent, TMiddleware>(brokerName, @switch);

            if (factory is not default(Func<IServiceProvider, TMiddleware>))
                _serviceCollection.TryAdd(ServiceDescriptor.Describe(typeof(TMiddleware), factory, lifetime));
            else
                _serviceCollection.TryAdd(ServiceDescriptor.Describe(typeof(TMiddleware), typeof(TMiddleware), lifetime));
        }

        private void AddMiddlewareType<TEvent, TMiddleware>(BusName brokerName, Enum @switch)
            where TEvent : IEvent
            where TMiddleware : class, IEventMiddleware<TEvent>
        {
            var middlewareType = typeof(TMiddleware);
            var eventType = typeof(TEvent);

            if (eventMiddlewareTypes.TryGetValue((brokerName, @switch, eventType), out var middlewares) &&
                !middlewares.ToList().Contains(middlewareType))
            {
                if (middlewareType.IsGenericType && middlewareType.GetGenericTypeDefinition() == typeof(ConsumeContextAccessorInitializerMiddleware<>))
                {
                    var newBag = new ConcurrentBag<Type>();
                    newBag.Add(middlewareType);
                    middlewares.ToList().ForEach(newBag.Add);
                    middlewares.Clear();
                    newBag.ToList().ForEach(middlewares.Add);
                }
                else
                    middlewares.Add(middlewareType);
            }
            else
            {
                if (!eventMiddlewareTypes.TryAdd((brokerName, @switch, eventType), new ConcurrentBag<Type> { middlewareType }))
                    throw new InvalidOperationException($"Não foi possível registrar o middleware do tipo '{middlewareType.FullName}'");
            }
        }
    }
}
