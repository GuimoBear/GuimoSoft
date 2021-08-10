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

        public abstract object GetMessage();
    }

    public sealed class ConsumeContext<TMessage> : ConsumeContextBase
        where TMessage : IMessage
    {
        public TMessage Message { get; }

        public ConsumeContext(TMessage message, IServiceProvider services, ConsumeInformations informations, CancellationToken cancellationToken)
            : base(services, informations, cancellationToken)
        {
            Message = message;
        }

        public override object GetMessage()
            => Message;
    }
}
