using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Logger;

namespace GuimoSoft.Examples.Bus.Kafka.Handlers.HelloMessage
{
    public class HelloMessageHandler : INotificationHandler<Messages.HelloMessage>
    {
        private readonly IApiLogger<HelloMessageHandler> _logger;
        private readonly IConsumeContextAccessor<Messages.HelloMessage> _contextAccessor;

        public HelloMessageHandler(IApiLogger<HelloMessageHandler> logger, IConsumeContextAccessor<Messages.HelloMessage> contextAccessor)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        public async Task Handle(Messages.HelloMessage notification, CancellationToken cancellationToken)
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
