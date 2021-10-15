using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;

namespace GuimoSoft.Bus.Tests.Fakes
{

    internal class FakeEventProducer : IBusEventDispatcher
    {
        internal static readonly FakeEventProducer Instance = new();

        public Task Dispatch<TEvent>(string key, TEvent @event, Enum @switch, string endpoint, CancellationToken cancellationToken = default) where TEvent : IEvent
        {
            throw new NotImplementedException();
        }
    }
}
