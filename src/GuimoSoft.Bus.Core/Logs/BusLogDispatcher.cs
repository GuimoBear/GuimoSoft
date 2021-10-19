using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Core.Logs.Builder;
using GuimoSoft.Bus.Core.Logs.Builder.Stages;
using System;

namespace GuimoSoft.Bus.Core.Logs
{
    internal sealed class BusLogDispatcher : IBusLogDispatcher
    {
        private readonly IServiceProvider _services;

        public BusLogDispatcher(IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public ISwitchStage FromBus(BusName bus)
            => new BusLogDispatcherBuilder(_services, bus);
    }
}
