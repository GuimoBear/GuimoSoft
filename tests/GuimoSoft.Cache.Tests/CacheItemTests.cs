using FluentAssertions;
using GuimoSoft.Cache.Tests.Fakes;
using GuimoSoft.Cache.Utils;
using Xunit;

namespace GuimoSoft.Cache.Tests
{
    public class CacheItemTests
    {
        [Fact]
        public void ImplicitConstructorFacts()
        {
            CacheItem<FakeValue> sut = default;

            FakeValue value = sut;

            value
                .Should().BeNull();
        }
    }
}
