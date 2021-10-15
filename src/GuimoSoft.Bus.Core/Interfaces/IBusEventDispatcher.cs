using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Core.Interfaces
{
    internal interface IBusEventDispatcher
    {
        Task Dispatch<TEvent>(string key, TEvent @event, Enum @switch, string endpoint, CancellationToken cancellationToken = default)
            where TEvent : IEvent;
    }
}
