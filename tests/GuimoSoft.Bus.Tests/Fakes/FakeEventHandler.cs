using GuimoSoft.Bus.Abstractions.Consumer;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeEventnHandler : IEventHandler<FakeEvent>
    {
        public Task Handle(FakeEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class FakeEventNotificationHandler : INotificationHandler<FakeEvent>
    {
        public Task Handle(FakeEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
