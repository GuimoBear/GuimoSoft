using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace GuimoSoft.Logger.Tests
{
    public class ApiLoggerContextAccessorTests
    {
        [Fact]
        public void ContextFacts()
        {
            var sut = new ApiLoggerContextAccessor();

            sut.Context
                .Should().BeNull();

            var firstContext = new Dictionary<string, object>();

            sut.Context = firstContext;

            sut.Context
                .Should().BeSameAs(firstContext);

            var secondsContext = new Dictionary<string, object>();

            sut.Context = secondsContext;

            sut.Context
                .Should().BeSameAs(secondsContext);
        }
    }
}
