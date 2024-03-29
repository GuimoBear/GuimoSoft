﻿using GuimoSoft.Bus.Abstractions;
using System;
using System.Collections.Generic;

namespace GuimoSoft.Bus.Core.Logs
{
    public class BusLogEvent : IEvent
    {
        public BusName Bus { get; internal init; }
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public Enum? Switch { get; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public Finality Finality { get; internal init; }
        public string Endpoint { get; internal init; }

        public string Message { get; internal init; }
        public BusLogLevel Level { get; internal init; }
        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();

        public BusLogEvent(Enum @switch)
        {
            Switch = ServerName.Default.Equals(@switch) ? null : @switch;
        }
    }
}
