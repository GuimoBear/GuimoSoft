using System;
using System.Collections.Generic;
using System.Reflection;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Utils;

namespace GuimoSoft.Bus.Core.Listener
{
    public class EndpointListenerBuilder<TOptions>
        where TOptions : class, new()
    {
        private readonly ListenerBuilder<TOptions> _parent;
        private readonly BusName _busName;
        private readonly Enum _switch;
        private readonly BusSerializerManager _busSerializerManager;
        private readonly EventMiddlewareManager _middlewareManager;
        private readonly EventTypeCache _eventTypesCache;
        private readonly ICollection<Assembly> _assemblies;

        internal EndpointListenerBuilder(
            ListenerBuilder<TOptions> parent,
            BusName busName,
            Enum @switch,
            BusSerializerManager busSerializerManager,
            EventMiddlewareManager middlewareManager,
            EventTypeCache eventTypesCache,
            ICollection<Assembly> assemblies)
        {
            _parent = parent;
            _busName = busName;
            _switch = @switch;

            _busSerializerManager = busSerializerManager;
            _middlewareManager = middlewareManager;
            _eventTypesCache = eventTypesCache;
            _assemblies = assemblies;
        }

        public TypedEndpointListenerBuilder<TOptions, TEvent> OfType<TEvent>() where TEvent : IEvent
        {
            _assemblies.TryAddAssembly(typeof(TEvent).Assembly);
            return new TypedEndpointListenerBuilder<TOptions, TEvent>(_parent, _busName, _switch, _busSerializerManager, _middlewareManager, _eventTypesCache);
        }
    }
}
