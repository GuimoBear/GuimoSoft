using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class ChildFakeEventHandler : IEventHandler<ChildFakeEvent>, IEvent, IEquatable<string>
    {
        public Task Handle(ChildFakeEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public bool Equals(string other)
        {
            throw new NotImplementedException();
        }
    }

    public class ChildFakeEventThrowExceptionHandler : IEventHandler<ChildFakeEvent>
    {
        public Task Handle(ChildFakeEvent notification, CancellationToken cancellationToken)
        {
            throw new System.Exception();
        }
    }
}
