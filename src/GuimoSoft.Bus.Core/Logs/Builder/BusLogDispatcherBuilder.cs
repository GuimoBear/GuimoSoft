using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Internal.Middlewares;
using GuimoSoft.Bus.Core.Logs.Builder.Stages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Core.Logs.Builder
{
    internal sealed class BusLogDispatcherBuilder :
        ISwitchStage,
        IFinalityStage, 
        IListeningStage,
        IEndpointStage,
        IEventObjectInstance,
        IEndpointAfterEventReceivedStage,
        IWriteStage,
        IMessageStage,
        ILogLevelAndDataStage,
        IKeyValueStage,
        IBeforePublishStage,
        IPublishStage
    {
        private readonly IServiceProvider _services;

        private readonly BusName _bus;
        private Enum _switch;
        private Finality _finality;

        private Type _eventType;
        private object _eventObject;

        private string _endpoint;
        private string _event;
        private BusLogLevel _level;

        private string _currentDataKey;
        private readonly Dictionary<string, object> _data;

        internal BusLogDispatcherBuilder(IServiceProvider services, BusName bus)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _bus = bus;
            _data = new();
        }

        public IFinalityStage AndSwitch(Enum @switch)
        {
            _switch = @switch;
            return this;
        }

        public IListeningStage AndFinality(Finality finality)
        {
            _finality = finality;
            return this;
        }

        public IEndpointStage WhileListening()
            => this;

        public IWriteStage TheEndpoint(string endpoint)
        {
            _endpoint = endpoint;
            return this;
        }

        public IEventObjectInstance AfterReceived()
            => this;

        public IEndpointAfterEventReceivedStage TheEvent(object @event)
            => TheEvent(@event?.GetType(), @event);

        public IEndpointAfterEventReceivedStage TheEvent(Type eventType, object @event)
        {
            _eventObject = @event;
            _eventType = eventType;
            return this;
        }

        public IWriteStage FromEndpoint(string endpoint)
            => TheEndpoint(endpoint);

        public IMessageStage Write()
            => this;

        public ILogLevelAndDataStage Message(string @event)
        {
            _event = @event;
            return this;
        }

        public IKeyValueStage AndKey(string key)
        {
            _currentDataKey = key;
            return this;
        }

        public ILogLevelAndDataStage WithValue(object value)
        {
            _data[_currentDataKey] = value;
            return this;
        }

        public IBeforePublishStage With(BusLogLevel level)
        {
            _level = level;
            return this;
        }

        public IPublishStage Publish()
            => this;

        public async Task AnLog(CancellationToken cancellationToken = default)
        {
            var logEvent = new BusLogEvent(_switch)
            {
                Bus = _bus,
                Finality = _finality,
                Endpoint = _endpoint,
                Message = _event,
                Level = _level
            };

            foreach (var (key, value) in _data)
                logEvent.Data.Add(key, value);
            if (_eventType != default && Singletons.GetBusTypedLogEventContainingAnHandlerCollection().Contains(_eventType))
                await PublishTypedLogEvent(logEvent, cancellationToken);
            else
                await EventDispatcherInvokeAsync(logEvent, cancellationToken);
        }

        public async Task AnException(Exception exception, CancellationToken cancellationToken = default)
        {
            Validate(exception);
            var exceptionEvent = new BusExceptionEvent(_switch, exception)
            {
                Bus = _bus,
                Finality = _finality,
                Endpoint = _endpoint,
                Message = _event,
                Level = _level
            };

            foreach (var (key, value) in _data)
                exceptionEvent.Data.Add(key, value);
            if (_eventType != default && Singletons.GetBusTypedExceptionEventContainingAnHandlerCollection().Contains(_eventType))
                await PublishTypeExceptionEvent(exceptionEvent, cancellationToken);
            else
                await EventDispatcherInvokeAsync(exceptionEvent, cancellationToken);
        }

        private static void Validate(Exception exception)
        {
            if (exception is null)
                throw new ArgumentNullException(nameof(exception));
        }

        private async Task PublishTypedLogEvent(BusLogEvent logEvent, CancellationToken cancellationToken)
        {
            var typedLogEventFactory = DelegateCache.GetOrAddBusLogEventFactory(_eventType);
            var typedLogEvent = typedLogEventFactory(logEvent, _eventObject);

            await EventDispatcherInvokeAsync(typedLogEvent, cancellationToken);
        }

        private async Task PublishTypeExceptionEvent(BusExceptionEvent exceptionEvent, CancellationToken cancellationToken)
        {
            var typedExceptionEventFactory = DelegateCache.GetOrAddBusExceptionEventFactory(_eventType);
            var typedExceptionEvent = typedExceptionEventFactory(exceptionEvent, _eventObject);

            await EventDispatcherInvokeAsync(typedExceptionEvent, cancellationToken);
        }

        private async Task EventDispatcherInvokeAsync(object @event, CancellationToken cancellationToken)
        {
            var consumeContext = Pipeline.CreateContext(@event.GetType(), @event, _services, new ConsumeInformations(_bus, _switch, _endpoint), cancellationToken);
            var eventDispatcherMiddleware = _services.GetService(typeof(EventDispatcherMiddleware<>).MakeGenericType(@event.GetType()));
            var invokeAsyncFunc = DelegateCache.GetOrAddEventDispatcherInvokeAsync(@event.GetType());
            await invokeAsyncFunc(eventDispatcherMiddleware, consumeContext, empty);
        }

        private static Task empty() => Task.CompletedTask;
    }
}
