﻿using Microsoft.Extensions.DependencyInjection;
using System;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Core.Serialization;

namespace GuimoSoft.Bus.Core.Consumer
{
    public class TypedEndpointConsumerBuilder<TOptions, TMessage>
        where TOptions : class, new()
        where TMessage : IMessage
    {
        private readonly ConsumerBuilder<TOptions> _parent;
        private readonly BusName _busName;
        private readonly Enum _switch;
        private readonly BusSerializerManager _busSerializerManager;
        private readonly MessageMiddlewareManager _middlewareManager;
        private readonly MessageTypeCache _messageTypesCache;

        internal TypedEndpointConsumerBuilder(
            ConsumerBuilder<TOptions> parent,
            BusName busName,
            Enum @switch,
            BusSerializerManager busSerializerManager,
            MessageMiddlewareManager middlewareManager,
            MessageTypeCache messageTypesCache)
        {
            _parent = parent;
            _busName = busName;
            _switch = @switch;

            _busSerializerManager = busSerializerManager;
            _middlewareManager = middlewareManager;
            _messageTypesCache = messageTypesCache;
        }

        public TypedEndpointConsumerBuilder<TOptions, TMessage> WithSerializer(TypedSerializer<TMessage> typedSerializer)
        {
            _busSerializerManager.AddTypedSerializer(_busName, Finality.Consume, _switch, typedSerializer);

            return this;
        }

        public TypedEndpointConsumerBuilder<TOptions, TMessage> WithMiddleware<TMiddleware>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TMiddleware : class, IMessageMiddleware<TMessage>
        {
            _middlewareManager.Register<TMessage, TMiddleware>(BusName.Kafka, _switch, lifetime);
            return this;
        }

        public TypedEndpointConsumerBuilder<TOptions, TMessage> WithMiddleware<TMiddleware>(Func<IServiceProvider, TMiddleware> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TMiddleware : class, IMessageMiddleware<TMessage>
        {
            _middlewareManager.Register<TMessage, TMiddleware>(BusName.Kafka, _switch, factory, lifetime);
            return this;
        }

        public ConsumerBuilder<TOptions> FromEndpoint(string endpoint)
        {
            _messageTypesCache.Add(_busName, Finality.Consume, _switch, typeof(TMessage), endpoint);
            return _parent;
        }
    }
}
