using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class SecondFakeEvent : IEvent
    {
        public const string TOPIC_NAME = "fake-event";

        public SecondFakeEvent(string key, string someProperty)
        {
            Key = key;
            SomeProperty = someProperty;
        }

        public string Key { get; set; }

        public string SomeProperty { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is FakeEvent @event)
            {
                return string.Equals(Key, @event.Key) &&
                       string.Equals(SomeProperty, @event.SomeProperty);
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