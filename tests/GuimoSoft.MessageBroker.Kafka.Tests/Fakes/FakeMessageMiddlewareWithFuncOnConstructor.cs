using System;
using System.Threading.Tasks;
using GuimoSoft.MessageBroker.Abstractions;
using GuimoSoft.MessageBroker.Kafka.Consumer;

namespace GuimoSoft.MessageBroker.Kafka.Tests.Fakes
{
    class FakeMessageMiddlewareWithFuncOnConstructor : IMessageMiddleware<FakeMessage>
    {
        private readonly Func<ConsumptionContext<FakeMessage>, Task> _func;

        public FakeMessageMiddlewareWithFuncOnConstructor(Func<ConsumptionContext<FakeMessage>, Task> func)
        {
            _func = func;
        }

        public async Task InvokeAsync(ConsumptionContext<FakeMessage> message, Func<Task> next)
        {
            await _func(message);
            await next();
        }
    }
}
