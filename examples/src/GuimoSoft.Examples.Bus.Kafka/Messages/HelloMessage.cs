using GuimoSoft.Bus.Abstractions;
using System.Text.Json.Serialization;

namespace GuimoSoft.Examples.Bus.Kafka.Messages
{
    [MessageTopic("viapag.gateway.core-example")]
    public class HelloMessage : IMessage
    {
        [JsonPropertyName(nameof(Name))]
        public string Name { get; private set; }

        [JsonConstructor]
        public HelloMessage(string name)
        {
            Name = name;
        }
    }
}
