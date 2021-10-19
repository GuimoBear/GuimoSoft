using Microsoft.Extensions.DependencyInjection;
using System;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Core.Serialization;
using GuimoSoft.Bus.Core.Internal.Middlewares;

namespace GuimoSoft.Bus.Core.Listener
{
    public class TypedEndpointListenerBuilder<TOptions, TEvent>
        where TOptions : class, new()
        where TEvent : IEvent
    {
        private readonly ListenerBuilder<TOptions> _parent;
        private readonly BusName _busName;
        private readonly Enum _switch;
        private readonly BusSerializerManager _busSerializerManager;
        private readonly EventMiddlewareManager _middlewareManager;
        private readonly EventTypeCache _eventTypesCache;

        internal TypedEndpointListenerBuilder(
            ListenerBuilder<TOptions> parent,
            BusName busName,
            Enum @switch,
            BusSerializerManager busSerializerManager,
            EventMiddlewareManager middlewareManager,
            EventTypeCache eventTypesCache)
        {
            _parent = parent;
            _busName = busName;
            _switch = @switch;

            _busSerializerManager = busSerializerManager;
            _middlewareManager = middlewareManager;
            _eventTypesCache = eventTypesCache;
        }

        public TypedEndpointListenerBuilder<TOptions, TEvent> WithSerializer(TypedSerializer<TEvent> typedSerializer)
        {
            _busSerializerManager.AddTypedSerializer(_busName, Finality.Consume, _switch, typedSerializer);

            return this;
        }

        public TypedEndpointListenerBuilder<TOptions, TEvent> WithMiddleware<TMiddleware>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TMiddleware : class, IEventMiddleware<TEvent>
        {
            _middlewareManager.Register<TEvent, TMiddleware>(BusName.Kafka, _switch, lifetime);
            return this;
        }

        public TypedEndpointListenerBuilder<TOptions, TEvent> WithMiddleware<TMiddleware>(Func<IServiceProvider, TMiddleware> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TMiddleware : class, IEventMiddleware<TEvent>
        {
            _middlewareManager.Register<TEvent, TMiddleware>(BusName.Kafka, _switch, factory, lifetime);
            return this;
        }

        public TypedEndpointListenerBuilder<TOptions, TEvent> WithContextAccessor()
        {
            _middlewareManager.Register<TEvent, ConsumeContextAccessorInitializerMiddleware<TEvent>>(BusName.Kafka, _switch, ServiceLifetime.Singleton);
            return this;
        }

        public ListenerBuilder<TOptions> FromEndpoint(string endpoint)
        {
            _eventTypesCache.Add(_busName, Finality.Consume, _switch, typeof(TEvent), endpoint);
            return _parent;
        }
    }
}
