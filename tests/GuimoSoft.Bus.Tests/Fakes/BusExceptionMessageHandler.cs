using MediatR;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Core.Logs;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class BusExceptionEventHandler : INotificationHandler<BusExceptionEvent>
    {
        public Task Handle(BusExceptionEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
