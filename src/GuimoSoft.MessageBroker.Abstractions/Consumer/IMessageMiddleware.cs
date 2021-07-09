using System;
using System.Threading.Tasks;

namespace GuimoSoft.MessageBroker.Abstractions.Consumer
{
    public interface IMessageMiddleware<TType>
        where TType : IMessage
    {
        Task InvokeAsync(ConsumptionContext<TType> context, Func<Task> next);
    }
}
