using System;

namespace GuimoSoft.Core.Serialization.Interfaces
{
    public interface IDefaultSerializer
    {
        byte[] Serialize(object @event);

        object Deserialize(Type eventType, byte[] content);
    }
}
