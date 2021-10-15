using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class SecondFakeEventHandler : INotificationHandler<SecondFakeEvent>
    {
        public Task Handle(SecondFakeEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
