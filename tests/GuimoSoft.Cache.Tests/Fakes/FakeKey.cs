using System;

namespace GuimoSoft.Cache.Tests.Fakes
{
    public class FakeKey
    {
        private readonly string _parameter1;
        private readonly int _parameter2;

        public FakeKey(string parameter1, int parameter2)
        {
            _parameter1 = parameter1;
            _parameter2 = parameter2;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_parameter1, _parameter2);
        }

        public override bool Equals(object obj)
        {
            return obj is FakeKey key &&
                   key.GetHashCode().Equals(GetHashCode());
        }
    }
}
