using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using System;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeMessageMiddleware : IMessageMiddleware<FakeMessage>
    {
        public async Task InvokeAsync(ConsumptionContext<FakeMessage> message, Func<Task> next)
        {
            await next();
        }
    }
}
