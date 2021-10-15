using System.Text.Json.Serialization;

namespace GuimoSoft.Core
{
    public class Notification
    {
        [JsonPropertyName("field")]
        public string Field { get; }
        [JsonPropertyName("event")]
        public string Event { get; }
        [JsonPropertyName("value")]
        public object Value { get; }

        public Notification(string field, string @event, object value = null)
        {
            Field = field;
            Event = @event;
            Value = value;
        }
    }
}
