using MediatR;
using System;
using System.Collections.Generic;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Core.Logs
{
    public class BusTypedExceptionEvent<TEvent> : INotification
        where TEvent : IEvent
    {
        public BusName Bus { get; }
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public Enum? Switch { get; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public Finality Finality { get; }
        public string Endpoint { get; }
        public TEvent Event { get; }
        public string Message { get; }
        public BusLogLevel Level { get; }
        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();
        public Exception Exception { get; }

        public BusTypedExceptionEvent(BusExceptionEvent exceptionEvent, TEvent @event)
        {
            Bus = exceptionEvent.Bus;
            Switch = exceptionEvent.Switch;
            Finality = exceptionEvent.Finality;
            Endpoint = exceptionEvent.Endpoint;
            Event = @event;
            Message = exceptionEvent.Message;
            Level = exceptionEvent.Level;
            Exception = exceptionEvent.Exception;
            foreach (var (key, value) in exceptionEvent.Data)
                Data.Add(key, value);
        }
    }
}
