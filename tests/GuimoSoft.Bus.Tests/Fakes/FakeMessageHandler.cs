using MediatR;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions.Consumer;

namespace GuimoSoft.Bus.Tests.Fakes
{
    class FakeMessageHandler : INotificationHandler<MessageNotification<FakeMessage>>
    {
        public Task Handle(MessageNotification<FakeMessage> notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
