using System;
using System.Collections.Generic;

namespace GuimoSoft.Bus.Abstractions
{
    public abstract class ConsumptionContextBase
    {
        public abstract object GetMessage();
    }

    public sealed class ConsumptionContext<TMessage> : ConsumptionContextBase
        where TMessage : IMessage
    {
        public TMessage Message { get; }
        public IServiceProvider Services { get; }
        public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();

        public ConsumptionContext(TMessage message, IServiceProvider services)
        {
            Message = message;
            Services = services;
        }

        public override object GetMessage()
            => Message;
    }
}
