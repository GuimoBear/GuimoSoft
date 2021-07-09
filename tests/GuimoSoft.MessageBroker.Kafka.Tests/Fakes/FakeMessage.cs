using GuimoSoft.MessageBroker.Abstractions;

namespace GuimoSoft.MessageBroker.Kafka.Tests.Fakes
{
    [MessageTopic(TOPIC_NAME)]
    public class FakeMessage : IMessage
    {
        public const string TOPIC_NAME = "fake-message";

        public FakeMessage(string key, string someProperty)
        {
            Key = key;
            SomeProperty = someProperty;
        }

        public string Key { get; set; }

        public string SomeProperty { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is FakeMessage message)
            {
                return string.Equals(Key, message.Key) &&
                       string.Equals(SomeProperty, message.SomeProperty);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return string.GetHashCode(Key) +
                   string.GetHashCode(SomeProperty);
        }
    }
}