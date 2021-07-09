using System;

namespace GuimoSoft.Cache.Tests.Fakes
{
    public class FakeValue
    {
        public string Property1 { get; }
        public int Property2 { get; }

        public FakeValue(string property1, int property2)
        {
            Property1 = property1;
            Property2 = property2;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Property1, Property2);
        }

        public override bool Equals(object obj)
        {
            return obj is FakeValue value &&
                   value.GetHashCode().Equals(GetHashCode());
        }
    }
}
