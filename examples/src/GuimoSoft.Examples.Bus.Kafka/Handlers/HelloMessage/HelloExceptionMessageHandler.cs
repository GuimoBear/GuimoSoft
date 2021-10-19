using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Logger;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Examples.Bus.Kafka.Handlers.HelloEvent
{
    public class HelloExceptionEventHandler : IEventHandler<BusTypedExceptionEvent<Events.HelloEvent>>
    {
        private readonly IApiLogger<HelloExceptionEventHandler> _logger;

        public HelloExceptionEventHandler(IApiLogger<HelloExceptionEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(BusTypedExceptionEvent<Events.HelloEvent> notification, CancellationToken cancellationToken)
        {
            var builder = _logger
                   .ComPropriedade(nameof(notification.Bus), notification.Bus.ToString())
                   .ComPropriedade(nameof(notification.Switch), notification.Switch?.ToString())
                   .ComPropriedade(nameof(notification.Endpoint), notification.Endpoint)
                   .ComPropriedade(nameof(notification.Data), notification.Data);

            switch (notification.Level)
            {
                case BusLogLevel.Trace:
                    builder.Rastreio(notification.Message);
                    break;
                case BusLogLevel.Debug:
                    builder.Depuracao(notification.Message);
                    break;
                case BusLogLevel.Information:
                    builder.Informacao(notification.Message);
                    break;
                case BusLogLevel.Warning:
                    builder.Atencao(notification.Message);
                    break;
                case BusLogLevel.Error:
                    builder.Erro(notification.Message, notification.Exception);
                    break;
            }
            return Task.CompletedTask;
        }
    }
}
