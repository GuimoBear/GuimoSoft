using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Logs;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class BusExceptionEventHandler : IEventHandler<BusExceptionEvent>
    {
        public Task Handle(BusExceptionEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
