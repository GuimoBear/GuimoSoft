using FluentAssertions;
using Polly;
using GuimoSoft.Cache.Tests.Fakes;
using GuimoSoft.Cache.Utils;
using Xunit;

namespace GuimoSoft.Cache.Tests
{
    public class ValueFactoryTests
    {
        [Fact]
        public void When_GetDefault_Then_GetDefaultSingletonInstance()
        {
            var expected = DefaultValueFactoryProxy<FakeValue>.Instance;
            var actual = ValueFactory.Instance.Default<FakeValue>();

            ReferenceEquals(actual, expected)
                .Should().BeTrue();
        }
        [Fact]
        public void When_GetResilient_Then_CreateNewResilientFactory()
        {
            var policy = Policy.NoOpAsync<FakeValue>();

            var actual = ValueFactory.Instance.Resilient(policy);

            var message = new FakeValue("prop", 5);

            var value = actual.Produce(() => message);

            ReferenceEquals(message, value)
                .Should().BeTrue();
        }
    }
}
