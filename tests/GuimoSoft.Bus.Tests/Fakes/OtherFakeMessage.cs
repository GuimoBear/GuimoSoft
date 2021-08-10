using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Tests.Fakes
{
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