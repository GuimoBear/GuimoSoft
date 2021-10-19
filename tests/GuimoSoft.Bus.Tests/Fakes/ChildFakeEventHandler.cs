using GuimoSoft.Bus.Abstractions;
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

    public class ChildFakeEventThrowExceptionHandler : IEventHandler<ChildFakeEvent>, IEvent
    {
        public Task Handle(ChildFakeEvent notification, CancellationToken cancellationToken)
        {
            throw new System.Exception();
        }
    }
}
