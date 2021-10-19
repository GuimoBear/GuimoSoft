using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Logger;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Examples.Bus.Kafka.Handlers.Bus
{
    public class BusHandler : IEventHandler<BusLogEvent>, IEventHandler<BusExceptionEvent>
    {
        private readonly IApiLogger<BusHandler> _logger;

        public BusHandler(IApiLogger<BusHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(BusLogEvent notification, CancellationToken cancellationToken)
        {
            var builder = _logger
                   .ComPropriedade(nameof(notification.Data), notification.Data)
                   .ComPropriedade(nameof(notification.Level), notification.Level);

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
                    builder.Erro(notification.Message);
                    break;
            }

            return Task.CompletedTask;
        }

        public Task Handle(BusExceptionEvent notification, CancellationToken cancellationToken)
        {
            _logger
                .ComPropriedade(nameof(notification.Data), notification.Data)
                .ComPropriedade(nameof(notification.Level), notification.Level)
                .Erro(notification.Message, notification.Exception);

            return Task.CompletedTask;
        }
    }
}
