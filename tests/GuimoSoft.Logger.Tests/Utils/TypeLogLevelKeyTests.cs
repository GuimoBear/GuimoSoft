using FluentAssertions;
using Microsoft.Extensions.Logging;
using GuimoSoft.Logger.Utils;
using Xunit;

namespace GuimoSoft.Logger.Tests.Utils
{
    public class TypeLogLevelKeyTests
    {
        [Fact]
        public void TypeLogLevelKeyFacts()
        {
            var sut = new TypeLogLevelKey(typeof(TypeLogLevelKeyTests), LogLevel.Information);

            sut.Equals("")
                .Should().BeFalse();

            sut.Equals(new TypeLogLevelKey(typeof(TypeLogLevelKeyTests), LogLevel.Warning))
                .Should().BeFalse();

            sut.Equals(new TypeLogLevelKey(typeof(TypeLogLevelKeyTests), LogLevel.Information))
                .Should().BeTrue();
        }
    }
}
