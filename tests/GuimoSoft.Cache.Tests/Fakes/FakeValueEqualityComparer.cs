using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GuimoSoft.Cache.Tests.Fakes
{
    public class FakeValueEqualityComparer : IEqualityComparer<FakeValue>
    {
        public static readonly IEqualityComparer<FakeValue> Instance = new FakeValueEqualityComparer();

        private FakeValueEqualityComparer()
        {

        }

        public bool Equals(FakeValue x, FakeValue y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;
            var equals = x.Equals(y);
            return equals;
        }

        public int GetHashCode([DisallowNull] FakeValue obj)
            => obj.GetHashCode();
    }
}
