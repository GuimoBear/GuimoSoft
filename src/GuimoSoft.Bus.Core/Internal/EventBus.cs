using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;

namespace GuimoSoft.Bus.Core.Internal
{
    internal sealed class EventBus : IEventBus
    {
        private readonly IDispatcherManager _producerManager;
        private readonly IEventTypeCache _eventTypeCache;
        private readonly IServiceProvider _services;

        public EventBus(IDispatcherManager producerManager, IEventTypeCache eventTypeCache, IServiceProvider services)
        {
            _producerManager = producerManager ?? throw new ArgumentNullException(nameof(producerManager));
            _eventTypeCache = eventTypeCache ?? throw new ArgumentNullException(nameof(eventTypeCache));
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public async Task Publish<TEvent>(string key, TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
        {
            ValidateParameters(key, @event);
            foreach (var (busName, @switch, endpoint) in _eventTypeCache.Get(@event.GetType()))
            {
                await _producerManager
                    .GetDispatcher(busName, _services)
                    .Dispatch(key, @event, @switch, endpoint, cancellationToken);
            }
        }

        private static void ValidateParameters<TEvent>(string key, TEvent @event) where TEvent : IEvent
        {
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("É necessário informar uma chave para enviar a mensagem", nameof(key));
        }
    }
}
