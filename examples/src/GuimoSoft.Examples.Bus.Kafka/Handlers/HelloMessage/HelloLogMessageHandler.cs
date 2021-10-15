﻿using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Logger;

namespace GuimoSoft.Examples.Bus.Kafka.Handlers.HelloEvent
{
    public class HelloLogEventHandler : INotificationHandler<BusTypedLogEvent<Events.HelloEvent>>
    {
        private readonly IApiLogger<HelloLogEventHandler> _logger;

        public HelloLogEventHandler(IApiLogger<HelloLogEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(BusTypedLogEvent<Events.HelloEvent> notification, CancellationToken cancellationToken)
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
                    builder.Erro(notification.Message);
                    break;
            }
            return Task.CompletedTask;
        }
    }
}
