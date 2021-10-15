using System;

namespace GuimoSoft.Core.Serialization.Interfaces
{
    internal interface IEventSerializerManager
    {
        IDefaultSerializer GetSerializer(Type eventType);
    }
}
