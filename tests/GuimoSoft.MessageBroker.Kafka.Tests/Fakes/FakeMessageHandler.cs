using MediatR;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.MessageBroker.Abstractions.Consumer;

namespace GuimoSoft.MessageBroker.Kafka.Tests.Fakes
{
    class FakeMessageHandler : INotificationHandler<MessageNotification<FakeMessage>>
    {
        public Task Handle(MessageNotification<FakeMessage> notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
