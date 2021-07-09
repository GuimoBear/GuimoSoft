using GuimoSoft.MessageBroker.Abstractions;

namespace GuimoSoft.MessageBroker.Kafka.Tests.Fakes
{
    [MessageTopic(TOPIC_NAME)]
    public class AnotherFakeMessage : IMessage
    {
        public const string TOPIC_NAME = "another-fake-message";

        public AnotherFakeMessage(string key, string someAnotherProperty)
        {
            Key = key;
            SomeAnotherProperty = someAnotherProperty;
        }

        public string Key { get; set; }

        public string SomeAnotherProperty { get; set; }
    }
}