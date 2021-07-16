using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using System;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeMessageThrowExceptionMiddleware : IMessageMiddleware<FakeMessage>
    {
        public Task InvokeAsync(ConsumptionContext<FakeMessage> message, Func<Task> next)
        {
            throw new Exception();
        }
    }
}
