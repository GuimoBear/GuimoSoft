using System;
using System.Collections.Generic;
using System.Reflection;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Utils;

namespace GuimoSoft.Bus.Core.Publisher
{
    public class EndpointPublisherBuilder<TOptions>
        where TOptions : class, new()
    {
        private readonly PublisherBuilder<TOptions> _parent;
        private readonly BusName _busName;
        private readonly Enum _switch;
        private readonly BusSerializerManager _busSerializerManager;
        private readonly EventTypeCache _eventTypesCache;
        private readonly ICollection<Assembly> _assemblies;

        internal EndpointPublisherBuilder(
            PublisherBuilder<TOptions> parent,
            BusName busName,
            Enum @switch,
            BusSerializerManager busSerializerManager,
            EventTypeCache eventTypesCache,
            ICollection<Assembly> assemblies)
        {
            _parent = parent;
            _busName = busName;
            _switch = @switch;

            _busSerializerManager = busSerializerManager;
            _eventTypesCache = eventTypesCache;
            _assemblies = assemblies;
        }

        public TypedEndpointPublisherBuilder<TOptions, TEvent> OfType<TEvent>() where TEvent : IEvent
        {
            _assemblies.TryAddAssembly(typeof(TEvent).Assembly);
            return new TypedEndpointPublisherBuilder<TOptions, TEvent>(_parent, _busName, _switch, _busSerializerManager, _eventTypesCache);
        }
    }
}
