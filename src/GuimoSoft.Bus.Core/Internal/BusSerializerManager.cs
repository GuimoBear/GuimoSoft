using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Core.Serialization;
using GuimoSoft.Core.Serialization.Interfaces;

namespace GuimoSoft.Bus.Core.Internal
{
    internal class BusSerializerManager : IBusSerializerManager
    {
        private readonly IDictionary<(BusName, Finality, Enum), EventSerializerManager> _serializerDictionary;

        public BusSerializerManager()
        {
            _serializerDictionary = new ConcurrentDictionary<(BusName, Finality, Enum), EventSerializerManager>();
        }

        public void AddTypedSerializer<TEvent>(BusName busName, Finality finality, Enum @switch, TypedSerializer<TEvent> serializer) where TEvent : IEvent
        {
            GetOrAdd(busName, finality, @switch).AddTypedSerializer(serializer);
        }

        public void SetDefaultSerializer(BusName busName, Finality finality, Enum @switch, IDefaultSerializer defaultSerializer)
        {
            GetOrAdd(busName, finality, @switch).SetDefaultSerializer(defaultSerializer);
        }

        public IDefaultSerializer GetSerializer(BusName busName, Finality finality, Enum @switch, Type eventType)
        {
            if (!_serializerDictionary.TryGetValue((busName, finality, @switch), out var serializerManager))
                return EventSerializerManager.Instance.GetSerializer(eventType);
            return serializerManager.GetSerializer(eventType);
        }

        public EventSerializerManager GetOrAdd(BusName busName, Finality finality, Enum @switch)
        {
            if (!_serializerDictionary.TryGetValue((busName, finality, @switch), out var serializerManager))
            {
                serializerManager = new EventSerializerManager();
                _serializerDictionary.TryAdd((busName, finality, @switch), serializerManager);
            }
            return serializerManager;
        }
    }
}
