using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Core.Interfaces
{
    internal interface IBusMessageProducer
    {
        Task ProduceAsync<TMessage>(string key, TMessage message, Enum @switch, string endpoint, CancellationToken cancellationToken = default)
            where TMessage : IMessage;
    }
}
