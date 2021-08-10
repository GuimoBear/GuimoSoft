using System;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Abstractions.Consumer
{
    public interface IMessageMiddleware<TMessage>
        where TMessage : IMessage
    {
        Task InvokeAsync(ConsumeContext<TMessage> context, Func<Task> next);
    }
}
