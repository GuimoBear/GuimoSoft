﻿using GuimoSoft.Bus.Abstractions;
using System;
using System.Collections.Generic;

namespace GuimoSoft.Bus.Core.Logs
{
    public class BusTypedLogEvent<TEvent> : IEvent
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

        public BusTypedLogEvent(BusLogEvent logEvent, TEvent @event)
        {
            Bus = logEvent.Bus;
            Switch = logEvent.Switch;
            Finality = logEvent.Finality;
            Endpoint = logEvent.Endpoint;
            Event = @event;
            Message = logEvent.Message;
            Level = logEvent.Level;
            foreach (var (key, value) in logEvent.Data)
                Data.Add(key, value);
        }
    }
}
