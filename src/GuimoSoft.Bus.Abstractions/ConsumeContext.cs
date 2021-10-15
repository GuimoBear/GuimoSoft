using System;
using System.Collections.Generic;
using System.Threading;

namespace GuimoSoft.Bus.Abstractions
{
    public abstract class ConsumeContextBase
    {
        public IServiceProvider Services { get; }
        public ConsumeInformations Informations { get; }
        public CancellationToken CancellationToken { get; }
        public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();

        protected ConsumeContextBase(IServiceProvider services, ConsumeInformations informations, CancellationToken cancellationToken)
        {
            Services = services;
            Informations = informations;
            CancellationToken = cancellationToken;
        }

        public abstract object GeTEvent();
    }

    public sealed class ConsumeContext<TEvent> : ConsumeContextBase
        where TEvent : IEvent
    {
        public TEvent Event { get; }

        public ConsumeContext(TEvent @event, IServiceProvider services, ConsumeInformations informations, CancellationToken cancellationToken)
            : base(services, informations, cancellationToken)
        {
            Event = @event;
        }

        public override object GeTEvent()
            => Event;
    }
}
