using System;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Core.Interfaces
{
    internal interface IProducerManager
    {
        IBusMessageProducer GetProducer(BusName busName, IServiceProvider services);
    }
}
