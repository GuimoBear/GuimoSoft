using MediatR;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Core.Logs;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeEventExceptionHandler : INotificationHandler<BusTypedExceptionEvent<FakeEvent>>
    {
        public Task Handle(BusTypedExceptionEvent<FakeEvent> notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
