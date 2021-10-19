using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Logs;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class ChildFakeEventExceptionHandler : IEventHandler<BusTypedExceptionEvent<ChildFakeEvent>>
    {
        public Task Handle(BusTypedExceptionEvent<ChildFakeEvent> notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
