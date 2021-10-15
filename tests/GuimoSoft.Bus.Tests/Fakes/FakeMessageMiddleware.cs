using System;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeEventMiddleware : IEventMiddleware<FakeEvent>
    {
        public async Task InvokeAsync(ConsumeContext<FakeEvent> @event, Func<Task> next)
        {
            await next();
        }
    }
}
