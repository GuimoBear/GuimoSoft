using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.MessageBroker.Abstractions
{
    public interface IMessageProducer
    {
        Task ProduceAsync<TMessage>(string key, TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessage;
    }
}