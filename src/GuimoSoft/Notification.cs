using System.Text.Json.Serialization;

namespace GuimoSoft
{
    public class Notification
    {
        [JsonPropertyName("field")]
        public string Field { get; }
        [JsonPropertyName("message")]
        public string Message { get; }
        [JsonPropertyName("value")]
        public object Value { get; }

        public Notification(string field, string message, object value = null)
        {
            Field = field;
            Message = message;
            Value = value;
        }
    }
}
