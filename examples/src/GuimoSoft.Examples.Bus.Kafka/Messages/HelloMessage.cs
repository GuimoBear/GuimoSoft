using System.Text.Json.Serialization;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Examples.Bus.Kafka.Events
{
    public class HelloEvent : IEvent
    {
        public const string TOPIC_NAME = "topic-example";

        [JsonPropertyName(nameof(Name))]
        public string Name { get; private set; }

        [JsonPropertyName(nameof(ThrowException))]
        public bool ThrowException { get; private set; }

        [JsonConstructor]
        public HelloEvent(string name, bool throwException)
        {
            Name = name;
            ThrowException = throwException;
        }
    }
}
