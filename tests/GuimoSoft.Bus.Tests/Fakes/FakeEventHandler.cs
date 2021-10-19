using GuimoSoft.Bus.Abstractions.Consumer;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeEventHandler : IEventHandler<FakeEvent>
    {
        public Task Handle(FakeEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class FakeEventThrowExceptionHandler : IEventHandler<FakeEvent>
    {
        public Task Handle(FakeEvent notification, CancellationToken cancellationToken)
        {
            throw new System.Exception();
        }
    }
}
