using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Logger;

namespace GuimoSoft.Examples.Bus.Kafka.Handlers.HelloEvent
{
    public class HelloEventHandler : INotificationHandler<Events.HelloEvent>
    {
        private readonly IApiLogger<HelloEventHandler> _logger;
        private readonly IConsumeContextAccessor<Events.HelloEvent> _contextAccessor;

        public HelloEventHandler(IApiLogger<HelloEventHandler> logger, IConsumeContextAccessor<Events.HelloEvent> contextAccessor)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        public async Task Handle(Events.HelloEvent notification, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            var informations = new Dictionary<string, object>
            {
                { nameof(_contextAccessor.Context.Informations.Bus), _contextAccessor.Context.Informations.Bus.ToString() },
                { nameof(_contextAccessor.Context.Informations.Switch), _contextAccessor.Context.Informations.Switch?.ToString() },
                { nameof(_contextAccessor.Context.Informations.Endpoint), _contextAccessor.Context.Informations.Endpoint },
                { nameof(_contextAccessor.Context.Informations.Headers), _contextAccessor.Context.Informations.Headers }
            };

            _logger
                .ComPropriedade("name", notification.Name)
                .ComPropriedade("informations", informations)
                .ComPropriedade("timestamp", DateTime.Now)
                .Informacao($"Hello {notification.Name}!");
        }
    }
}
