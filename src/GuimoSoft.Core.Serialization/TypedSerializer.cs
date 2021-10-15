using System;
using GuimoSoft.Core.Serialization.Interfaces;

namespace GuimoSoft.Core.Serialization
{
    public abstract class TypedSerializer<TEvent> : IDefaultSerializer
    {
        protected abstract byte[] Serialize(TEvent @event);

        protected abstract TEvent Deserialize(byte[] content);

        public byte[] Serialize(object @event)
            => Serialize((TEvent)@event);

        public object Deserialize(Type eventType, byte[] content)
            => Deserialize(content);
    }
}
