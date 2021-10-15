using GuimoSoft.Bus.Abstractions.Consumer;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class SecondFakeEventHandler : IEventHandler<SecondFakeEvent>
    {
        public Task Handle(SecondFakeEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class SecondFakeEventNotificationHandler : INotificationHandler<SecondFakeEvent>
    {
        public Task Handle(SecondFakeEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
