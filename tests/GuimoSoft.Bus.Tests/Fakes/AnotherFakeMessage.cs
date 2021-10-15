using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class AnotherFakeEvent : IEvent
    {
        public const string TOPIC_NAME = "another-fake-event";

        public AnotherFakeEvent(string key, string someAnotherProperty)
        {
            Key = key;
            SomeAnotherProperty = someAnotherProperty;
        }

        public string Key { get; set; }

        public string SomeAnotherProperty { get; set; }
    }
}