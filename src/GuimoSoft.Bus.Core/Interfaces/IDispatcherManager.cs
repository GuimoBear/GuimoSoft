using System;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Core.Interfaces
{
    internal interface IDispatcherManager
    {
        IBusEventDispatcher GetDispatcher(BusName busName, IServiceProvider services);
    }
}
