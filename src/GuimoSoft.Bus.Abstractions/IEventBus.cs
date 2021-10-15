using System;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Abstractions
{
    public interface IEventBus
    {
        Task Publish<TEvent>(string key, TEvent @event, CancellationToken cancellationToken = default) 
            where TEvent : IEvent;
    }
}