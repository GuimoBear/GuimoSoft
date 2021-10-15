using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GuimoSoft.Core.Serialization.Interfaces;

namespace GuimoSoft.Core.Serialization
{
    internal sealed class EventSerializerManager : IEventSerializerManager
    {
        internal static readonly EventSerializerManager Instance
            = new EventSerializerManager();

        private IDefaultSerializer _defaultSerializer = JsonEventSerializer.Instance;
        private readonly IDictionary<Type, IDefaultSerializer> _typedSerializers
            = new ConcurrentDictionary<Type, IDefaultSerializer>();

        internal void SetDefaultSerializer(IDefaultSerializer defaultSerializer)
        {
            _defaultSerializer = defaultSerializer ?? throw new ArgumentNullException(nameof(defaultSerializer));
        }

        internal void AddTypedSerializer<TEvent>(TypedSerializer<TEvent> serializer)
        {
            if (serializer is null)
                throw new ArgumentNullException(nameof(serializer));
            _typedSerializers[typeof(TEvent)] = serializer;
        }

        public IDefaultSerializer GetSerializer(Type eventType)
        {
            if (_typedSerializers.TryGetValue(eventType, out var serializer))
                return serializer;
            return _defaultSerializer;
        }
    }
}
