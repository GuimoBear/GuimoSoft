using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;

namespace GuimoSoft.Bus.Tests.Fakes
{

    internal class FakeMessageProducer : IBusMessageProducer
    {
        internal static readonly FakeMessageProducer Instance = new();

        public Task ProduceAsync<TMessage>(string key, TMessage message, Enum @switch, string endpoint, CancellationToken cancellationToken = default) where TMessage : IMessage
        {
            throw new NotImplementedException();
        }
    }
}
