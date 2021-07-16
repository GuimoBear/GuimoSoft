using GuimoSoft.Bus.Abstractions.Consumer;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeMessageHandler : INotificationHandler<MessageNotification<FakeMessage>>
    {
        public Task Handle(MessageNotification<FakeMessage> notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
