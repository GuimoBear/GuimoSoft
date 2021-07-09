using System;
using System.Threading.Tasks;
using GuimoSoft.MessageBroker.Abstractions;
using GuimoSoft.MessageBroker.Kafka.Consumer;

namespace GuimoSoft.MessageBroker.Kafka.Tests.Fakes
{
    public class FakeMessageMiddleware : IMessageMiddleware<FakeMessage>
    {
        public async Task InvokeAsync(ConsumptionContext<FakeMessage> message, Func<Task> next)
        {
            await next();
        }
    }
}
