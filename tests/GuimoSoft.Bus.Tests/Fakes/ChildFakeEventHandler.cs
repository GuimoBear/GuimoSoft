using GuimoSoft.Bus.Abstractions.Consumer;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class ChildFakeEventHandler : IEventHandler<ChildFakeEvent>
    {
        public Task Handle(ChildFakeEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
