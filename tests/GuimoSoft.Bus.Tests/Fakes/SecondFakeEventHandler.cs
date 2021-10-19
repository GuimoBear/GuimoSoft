using GuimoSoft.Bus.Abstractions.Consumer;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class SecondFakeEventHandler : IEventHandler<SecondFakeEvent>
    {
        public Task Handle(SecondFakeEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
