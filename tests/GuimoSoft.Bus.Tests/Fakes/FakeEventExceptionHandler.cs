using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Logs;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeEventExceptionHandler : IEventHandler<BusTypedExceptionEvent<FakeEvent>>
    {
        public Task Handle(BusTypedExceptionEvent<FakeEvent> notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
