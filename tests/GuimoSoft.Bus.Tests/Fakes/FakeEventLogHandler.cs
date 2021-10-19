using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Logs;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeEventLogHandler : IEventHandler<BusTypedLogEvent<FakeEvent>>
    {
        public Task Handle(BusTypedLogEvent<FakeEvent> notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
