using System;

namespace GuimoSoft.Serialization.Interfaces
{
    internal interface IMessageSerializerManager
    {
        IDefaultSerializer GetSerializer(Type messageType);
    }
}
