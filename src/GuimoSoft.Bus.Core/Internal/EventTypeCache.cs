using System;
using System.Collections.Generic;
using System.Linq;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;

namespace GuimoSoft.Bus.Core.Internal
{
    internal class EventTypeCache : IEventTypeCache
    {
        private readonly object _lock = new();

        private readonly ICollection<EventTypeItem> _eventTypes;

        public EventTypeCache()
        {
            _eventTypes = new List<EventTypeItem>();
        }

        public void Add(BusName busName, Finality finality, Enum @switch, Type type, string endpoint)
        {
            if (!typeof(IEvent).IsAssignableFrom(type))
                throw new ArgumentException($"{type.Name} deve implementar a interface {nameof(IEvent)}");
            if (@switch is null)
                throw new ArgumentNullException(nameof(@switch));
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("É necessário informar um endpoint para haver o registro", nameof(endpoint));

            var item = new EventTypeItem(busName, finality, @switch, type, endpoint);
            lock (_lock)
            {
                if (!_eventTypes.Contains(item))
                    _eventTypes.Add(item);
            }
        }

        public IEnumerable<Enum> GetSwitchers(BusName busName, Finality finality)
        {
            var items = _eventTypes
                .Where(mt => mt.Bus.Equals(busName) && mt.Finality.Equals(finality))
                .Select(mt => mt.Switch)
                .Distinct()
                .ToList();
            if (items.Count == 0)
                throw new InvalidOperationException($"Não existem switches para o bus {busName}");
            return items;
        }

        public IEnumerable<string> GetEndpoints(BusName busName, Finality finality, Enum @switch)
        {
            if (@switch is null)
                throw new ArgumentNullException(nameof(@switch));
            var items = _eventTypes
                .Where(mt => mt.Bus.Equals(busName) && mt.Finality.Equals(finality) && mt.Switch.Equals(@switch))
                .Select(mt => mt.Endpoint)
                .ToList();

            if (items.Count == 0)
                throw new KeyNotFoundException($"Não existem endpoints configurados para o bus {busName} e para o switch {@switch}");
            return items;
        }

        public IEnumerable<string> Get(BusName busName, Finality finality, Enum @switch, IEvent @event)
        {
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));
            var eventType = @event.GetType();

            return Get(busName, finality, @switch, eventType);
        }

        public IEnumerable<string> Get(BusName busName, Finality finality, Enum @switch, Type eventType)
        {
            if (eventType is null)
                throw new ArgumentNullException(nameof(eventType));
            if (@switch is null)
                throw new ArgumentNullException(nameof(@switch));

            var items = _eventTypes
                .Where(mt => mt.Bus.Equals(busName) && mt.Finality.Equals(finality) && mt.Switch.Equals(@switch) && mt.Type.Equals(eventType))
                .Select(mt => mt.Endpoint)
                .ToList();
            if (items.Count == 0)
                throw new KeyNotFoundException($"Não existem endpoints configurados para o bus {busName} e para o switch {@switch}");
            return items;
        }

        public IReadOnlyCollection<Type> Get(BusName busName, Finality finality, Enum @switch, string endpoint)
        {
            if (@switch is null)
                throw new ArgumentNullException(nameof(@switch));
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("É necessário informar um endpoint obter os tipos da mensagem", nameof(endpoint));

            var items = _eventTypes
                .Where(mt => mt.Bus.Equals(busName) && mt.Finality.Equals(finality) && mt.Switch.Equals(@switch) && mt.Endpoint.Equals(endpoint))
                .Select(mt => mt.Type)
                .ToList();
            if (items.Count == 0)
                throw new KeyNotFoundException($"Não existem endpoints configurados para o bus {busName} e para o switch {@switch}");
            return items;
        }

        public IEnumerable<(BusName BusName, Enum Switch, string Endpoint)> Get(Type eventType)
        {
            if (eventType is null)
                throw new ArgumentNullException(nameof(eventType));

            var items = _eventTypes
                   .Where(mt => mt.Finality.Equals(Finality.Produce) && mt.Type.Equals(eventType))
                   .Select(mt => (mt.Bus, mt.Switch, mt.Endpoint))
                   .ToList();

            if (items.Count == 0)
                throw new KeyNotFoundException($"Não existem endpoints configurados para a mensagem do tipo {eventType}");
            return items;
        }

        internal sealed class EventTypeItem
        {
            public BusName Bus { get; }
            public Finality Finality { get; }
            public Enum Switch { get; }
            public Type Type { get; }
            public string Endpoint { get; }

            private readonly int _hashCode;

            public EventTypeItem(BusName bus, Finality finality, Enum @switch, Type type, string endpoint)
            {
                Bus = bus;
                Finality = finality;
                Switch = @switch;
                Type = type;
                Endpoint = endpoint;

                _hashCode = HashCode.Combine(Bus, Finality, Switch, Type, Endpoint);
            }

            public override bool Equals(object obj)
            {
                return obj is EventTypeItem item &&
                       item.GetHashCode().Equals(GetHashCode());
            }

            public override int GetHashCode()
                => _hashCode;
        }
    }
}
