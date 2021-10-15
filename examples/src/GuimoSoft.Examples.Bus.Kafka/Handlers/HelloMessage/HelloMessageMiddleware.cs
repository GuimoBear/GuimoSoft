using System;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Logger;

namespace GuimoSoft.Examples.Bus.Kafka.Handlers.HelloEvent
{
    public class HelloEventMiddleware : IEventMiddleware<Events.HelloEvent>
    {
        private readonly IApiLogger<HelloEventMiddleware> _logger;

        public HelloEventMiddleware(IApiLogger<HelloEventMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(ConsumeContext<Events.HelloEvent> context, Func<Task> next)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            _logger
                .ComPropriedade("name", context.Event.Name)
                .ComPropriedade("timestamp", DateTime.Now)
                .Informacao($"Middleware");

            if (context.Event.ThrowException)
                throw new ArgumentException(nameof(context));

            await next();
        }
    }
}
