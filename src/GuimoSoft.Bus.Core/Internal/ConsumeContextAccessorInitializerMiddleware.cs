using System;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;

namespace GuimoSoft.Bus.Core.Internal
{
    internal sealed class ConsumeContextAccessorInitializerMiddleware<TMessage> : IMessageMiddleware<TMessage>
        where TMessage : IMessage
    {
        private readonly IConsumeContextAccessor<TMessage> contextAccessor;

        public ConsumeContextAccessorInitializerMiddleware(IConsumeContextAccessor<TMessage> contextAccessor)
        {
            this.contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public async Task InvokeAsync(ConsumeContext<TMessage> context, Func<Task> next)
        {
            try
            {
                contextAccessor.Context = context;
                await next();
            }
            finally
            {
                contextAccessor.Context = null;
            }
        }
    }
}
