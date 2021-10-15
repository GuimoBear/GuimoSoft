using MediatR;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Core.Logs;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class BusLogEventHandler : INotificationHandler<BusLogEvent>
    {
        public Task Handle(BusLogEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
