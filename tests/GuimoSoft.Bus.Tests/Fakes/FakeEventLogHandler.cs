using MediatR;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Core.Logs;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeEventLogHandler : INotificationHandler<BusTypedLogEvent<FakeEvent>>
    {
        public Task Handle(BusTypedLogEvent<FakeEvent> notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
