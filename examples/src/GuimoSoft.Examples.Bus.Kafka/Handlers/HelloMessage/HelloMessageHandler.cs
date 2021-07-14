using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Logger;

namespace GuimoSoft.Examples.Bus.Kafka.Handlers.HelloMessage
{
    public class HelloMessageHandler : INotificationHandler<MessageNotification<Messages.HelloMessage>>
    {
        private readonly IApiLogger<HelloMessageHandler> _logger;

        public HelloMessageHandler(IApiLogger<HelloMessageHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(MessageNotification<Messages.HelloMessage> notification, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            _logger
                .ComPropriedade("name", notification.Message.Name)
                .ComPropriedade("timestamp", DateTime.Now)
                .Informacao($"Hello {notification.Message.Name}!");
        }
    }
}
