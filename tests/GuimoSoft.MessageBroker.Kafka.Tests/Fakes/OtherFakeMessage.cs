using GuimoSoft.MessageBroker.Abstractions;

namespace GuimoSoft.MessageBroker.Kafka.Tests.Fakes
{
    [MessageTopic(TOPIC_NAME)]
    public class OtherFakeMessage : IMessage
    {
        public const string TOPIC_NAME = "other-fake-message";

        public OtherFakeMessage(string key, string someOtherProperty)
        {
            Key = key;
            SomeOtherProperty = someOtherProperty;
        }

        public string Key { get; set; }

        public string SomeOtherProperty { get; set; }
    }
}