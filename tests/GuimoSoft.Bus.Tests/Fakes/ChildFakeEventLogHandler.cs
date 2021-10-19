using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Logs;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class ChildFakeEventLogHandler : IEventHandler<BusTypedLogEvent<ChildFakeEvent>>
    {
        public Task Handle(BusTypedLogEvent<ChildFakeEvent> notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
