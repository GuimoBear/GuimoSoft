using System;
using System.Text;
using System.Text.Json;
using GuimoSoft.Core.Serialization.Interfaces;

namespace GuimoSoft.Core.Serialization
{
    internal sealed class JsonEventSerializer : IDefaultSerializer
    {
        public static readonly IDefaultSerializer Instance = new JsonEventSerializer();

        private JsonEventSerializer() { }

        public byte[] Serialize(object @event)
        {
            return JsonSerializer.SerializeToUtf8Bytes(@event);
        }

        public object Deserialize(Type eventType, byte[] content)
        {
            var stringContent = Encoding.UTF8.GetString(content);
            return JsonSerializer.Deserialize(stringContent, eventType);
        }
    }
}
