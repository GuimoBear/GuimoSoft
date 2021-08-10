using System;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Abstractions
{
    public interface IMessageProducer
    {
        Task ProduceAsync<TMessage>(string key, TMessage message, CancellationToken cancellationToken = default) 
            where TMessage : IMessage;
    }
}