using System;
using GuimoSoft.Core.Serialization.Interfaces;

namespace GuimoSoft.Core.Serialization
{
    public abstract class TypedSerializer<TMessage> : IDefaultSerializer
    {
        protected abstract byte[] Serialize(TMessage message);

        protected abstract TMessage Deserialize(byte[] content);

        public byte[] Serialize(object message)
            => Serialize((TMessage)message);

        public object Deserialize(Type messageType, byte[] content)
            => Deserialize(content);
    }
}
