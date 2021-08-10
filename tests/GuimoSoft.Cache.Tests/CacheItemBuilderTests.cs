using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GuimoSoft.Cache.Tests.Fakes;
using GuimoSoft.Cache.Utils;
using Xunit;

namespace GuimoSoft.Cache.Tests
{
    public class CacheItemBuilderTests
    {
        [Fact]
        public void BuildFacts()
        {
            CacheItemBuilder<string, FakeValue> sut = default;

            FakeValue actualValue = sut;

            actualValue
                .Should().BeNull();

            sut = new CacheItemBuilder<string, FakeValue>("teste", FakeReturnCachedValue, null, null); 
            
            actualValue = sut;

            actualValue
                .Should().BeNull();
        }

        private static bool FakeReturnCachedValue(string key, out CacheState cacheState, out FakeValue value)
        {
            cacheState = CacheState.NotFound;
            value = default;
            return false;
        }
    }
}
