namespace GuimoSoft.Bus.Tests.Fakes
{
    public class ChildFakeEvent : FakeEvent
    {
        public string AnotherProperty { get; set; }

        public ChildFakeEvent(string key, string someProperty, string anotherProperty) : base(key, someProperty)
        {
            AnotherProperty = anotherProperty;
        }
    }
}
