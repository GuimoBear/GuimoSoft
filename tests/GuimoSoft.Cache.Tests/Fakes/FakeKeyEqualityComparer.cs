using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GuimoSoft.Cache.Tests.Fakes
{
    public class FakeKeyEqualityComparer : IEqualityComparer<FakeKey>
    {
        public static readonly IEqualityComparer<FakeKey> Instance = new FakeKeyEqualityComparer();

        private FakeKeyEqualityComparer()
        {

        }

        public bool Equals(FakeKey x, FakeKey y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;
            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] FakeKey obj)
            => obj.GetHashCode();
    }
}
