﻿using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Logs;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class BusLogEventHandler : IEventHandler<BusLogEvent>
    {
        public Task Handle(BusLogEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
