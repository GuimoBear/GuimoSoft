using System;

namespace GuimoSoft.Bus.Core.Logs.Builder.Stages
{
    public interface IEventObjectInstance
    {
        IEndpointAfterEventReceivedStage TheEvent(object @event);
        IEndpointAfterEventReceivedStage TheEvent(Type eventType, object @event);
    }
}
