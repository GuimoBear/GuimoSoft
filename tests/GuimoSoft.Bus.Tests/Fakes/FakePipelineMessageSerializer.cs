using System.Text;
using System.Text.Json;
using GuimoSoft.Core.Serialization;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakePipelineEventSerializer : TypedSerializer<FakePipelineEvent>
    {
        public static TypedSerializer<FakePipelineEvent> Instance
               = new FakePipelineEventSerializer();

        private FakePipelineEventSerializer() { }

        protected override FakePipelineEvent Deserialize(byte[] content)
        {
            return JsonSerializer.Deserialize<FakePipelineEvent>(Encoding.UTF8.GetString(content));
        }

        protected override byte[] Serialize(FakePipelineEvent @event)
        {
            return JsonSerializer.SerializeToUtf8Bytes(@event);
        }
    }
}
