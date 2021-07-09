namespace GuimoSoft.Cryptography.Tests.Fakes
{
    public class FakeRequest
    {
        public string Property1 { get; }
        public int Property2 { get; }

        public FakeRequest(string property1, int property2)
        {
            Property1 = property1;
            Property2 = property2;
        }
    }
}
