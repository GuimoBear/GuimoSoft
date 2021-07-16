using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Logger;
using System;
using System.Threading.Tasks;

namespace GuimoSoft.Examples.Bus.Kafka.Handlers.HelloMessage
{
    public class HelloMessageMiddleware : IMessageMiddleware<Messages.HelloMessage>
    {
        private readonly IApiLogger<HelloMessageMiddleware> _logger;

        public HelloMessageMiddleware(IApiLogger<HelloMessageMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(ConsumptionContext<Messages.HelloMessage> context, Func<Task> next)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            _logger
                .ComPropriedade("name", context.Message.Name)
                .ComPropriedade("timestamp", DateTime.Now)
                .Informacao($"Middleware");
            await next();
        }
    }
}
