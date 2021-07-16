using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Core
{
    internal sealed class MediatorPublisherMiddleware<TMessage> : IMessageMiddleware<TMessage>
        where TMessage : IMessage
    {
        public async Task InvokeAsync(ConsumptionContext<TMessage> context, Func<Task> next)
        {
            var mediator = context.Services.GetRequiredService<IMediator>();
            await mediator.Publish(new MessageNotification<TMessage>(context.Message)).ConfigureAwait(false);
            await next();
        }
    }
}
