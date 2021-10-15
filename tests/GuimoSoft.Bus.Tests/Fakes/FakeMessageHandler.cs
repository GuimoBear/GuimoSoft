using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeEventHandler : INotificationHandler<FakeEvent>
    {
        public Task Handle(FakeEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
