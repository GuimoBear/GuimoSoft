using System;
using System.Threading.Tasks;
using GuimoSoft.MessageBroker.Abstractions;

namespace GuimoSoft.MessageBroker.Kafka.Consumer
{
    public interface IMessageMiddleware<TType>
        where TType : IMessage
    {
        Task InvokeAsync(ConsumptionContext<TType> context, Func<Task> next);
    }
}
