using System;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Core.Serialization;

namespace GuimoSoft.Bus.Core.Publisher
{
    public class TypedEndpointPublisherBuilder<TOptions, TEvent>
        where TOptions : class, new()
        where TEvent : IEvent
    {
        private readonly PublisherBuilder<TOptions> _parent;
        private readonly BusName _busName;
        private readonly Enum _switch;
        private readonly BusSerializerManager _busSerializerManager;
        private readonly EventTypeCache _eventTypesCache;

        internal TypedEndpointPublisherBuilder(
            PublisherBuilder<TOptions> parent,
            BusName busName,
            Enum @switch,
            BusSerializerManager busSerializerManager,
            EventTypeCache eventTypesCache)
        {
            _parent = parent;
            _busName = busName;
            _switch = @switch;

            _busSerializerManager = busSerializerManager;
            _eventTypesCache = eventTypesCache;
        }

        public TypedEndpointPublisherBuilder<TOptions, TEvent> WithSerializer(TypedSerializer<TEvent> typedSerializer)
        {
            _busSerializerManager.AddTypedSerializer(_busName, Finality.Produce, _switch, typedSerializer);

            return this;
        }

        public PublisherBuilder<TOptions> ToEndpoint(string endpoint)
        {
            _eventTypesCache.Add(_busName, Finality.Produce, _switch, typeof(TEvent), endpoint);
            return _parent;
        }
    }
}
