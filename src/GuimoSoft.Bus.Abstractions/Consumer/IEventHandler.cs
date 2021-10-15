﻿using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Abstractions.Consumer
{
    public interface IEventHandler<TEvent> where TEvent : IEvent
    {
        Task Handle(TEvent @event, CancellationToken cancellationToken);
    }
}
