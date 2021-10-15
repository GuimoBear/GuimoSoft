using System.Text;
using System.Text.Json;
using GuimoSoft.Core.Serialization;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class OtherFakeEventSerializer : TypedSerializer<OtherFakeEvent>
    {
        public static TypedSerializer<OtherFakeEvent> Instance
            = new OtherFakeEventSerializer();

        private OtherFakeEventSerializer() { }

        protected override OtherFakeEvent Deserialize(byte[] content)
        {
            return JsonSerializer.Deserialize<OtherFakeEvent>(Encoding.UTF8.GetString(content));
        }

        protected override byte[] Serialize(OtherFakeEvent @event)
        {
            return JsonSerializer.SerializeToUtf8Bytes(@event);
        }
    }
}
