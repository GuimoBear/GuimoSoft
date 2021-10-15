using System;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeEventThrowExceptionMiddleware : IEventMiddleware<FakeEvent>
    {
        public Task InvokeAsync(ConsumeContext<FakeEvent> @event, Func<Task> next)
        {
            throw new Exception();
        }
    }
}
