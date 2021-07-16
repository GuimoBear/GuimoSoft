using GuimoSoft.Bus.Abstractions.Consumer;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class SecondFakeMessageHandler : INotificationHandler<MessageNotification<SecondFakeMessage>>
    {
        public Task Handle(MessageNotification<SecondFakeMessage> notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
