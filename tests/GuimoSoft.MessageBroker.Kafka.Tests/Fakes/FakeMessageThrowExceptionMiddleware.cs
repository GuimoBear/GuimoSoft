using System;
using System.Threading.Tasks;
using GuimoSoft.MessageBroker.Abstractions;
using GuimoSoft.MessageBroker.Kafka.Consumer;

namespace GuimoSoft.MessageBroker.Kafka.Tests.Fakes
{
    public class FakeMessageThrowExceptionMiddleware : IMessageMiddleware<FakeMessage>
    {
        public Task InvokeAsync(ConsumptionContext<FakeMessage> message, Func<Task> next)
        {
            throw new Exception();
        }
    }
}
