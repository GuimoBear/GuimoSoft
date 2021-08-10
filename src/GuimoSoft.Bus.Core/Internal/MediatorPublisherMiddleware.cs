using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;

namespace GuimoSoft.Bus.Core.Internal
{
    internal sealed class MediatorPublisherMiddleware<TMessage> : IMessageMiddleware<TMessage>
        where TMessage : IMessage
    {
        public async Task InvokeAsync(ConsumeContext<TMessage> context, Func<Task> next)
        {
            var mediator = context.Services.GetRequiredService<IMediator>();
            await mediator.Publish(context.Message, context.CancellationToken).ConfigureAwait(false);
            await next();
        }
    }
}
