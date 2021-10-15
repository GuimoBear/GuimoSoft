using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class OtherFakeEvent : IEvent
    {
        public const string TOPIC_NAME = "other-fake-event";

        public OtherFakeEvent(string key, string someOtherProperty)
        {
            Key = key;
            SomeOtherProperty = someOtherProperty;
        }

        public string Key { get; set; }

        public string SomeOtherProperty { get; set; }
    }
}