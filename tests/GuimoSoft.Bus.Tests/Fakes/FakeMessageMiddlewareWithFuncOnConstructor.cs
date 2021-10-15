using System;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;

namespace GuimoSoft.Bus.Tests.Fakes
{
    internal class FakeEventMiddlewareWithFuncOnConstructor : IEventMiddleware<FakeEvent>
    {
        private readonly Func<ConsumeContext<FakeEvent>, Task> _func;

        public FakeEventMiddlewareWithFuncOnConstructor(Func<ConsumeContext<FakeEvent>, Task> func)
        {
            _func = func;
        }

        public async Task InvokeAsync(ConsumeContext<FakeEvent> @event, Func<Task> next)
        {
            await _func(@event);
            await next();
        }
    }
}
