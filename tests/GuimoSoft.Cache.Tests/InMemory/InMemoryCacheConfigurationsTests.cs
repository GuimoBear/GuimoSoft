using FluentAssertions;
using GuimoSoft.Cache.Tests.Fakes;
using Moq;
using System;
using Xunit;

namespace GuimoSoft.Cache.Tests.InMemory
{
    public class InMemoryCacheConfigurationsTests
    {
        private static readonly TimeSpan _ttl = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan _cleanerInterval = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan _cleanerCheckInterval = TimeSpan.FromMilliseconds(500);

        [Fact]
        public void When_TryBuildWithoutTtl_Then_ThrowException()
        {
            var sut = new InMemoryCacheConfigurations<FakeKey, FakeValue>.InMemoryCacheConfigurationsBuilder();

            Assert.Throws<ArgumentException>(sut.Build);
        }

        [Fact]
        public void When_TryConfigureWithDefautTtl_Then_ThrowException()
        {
            var sut = new InMemoryCacheConfigurations<FakeKey, FakeValue>.InMemoryCacheConfigurationsBuilder();

            Assert.Throws<ArgumentException>(() => sut.WithTTL(default));
        }

        [Fact]
        public void When_TryConfigureWithValidTtl_Then_BuildSuccessfully()
        {
            var sut = new InMemoryCacheConfigurations<FakeKey, FakeValue>.InMemoryCacheConfigurationsBuilder();

            sut.WithTTL(_ttl);

            var configs = sut.Build();

            configs.TTL
                .Should().Be(_ttl);
        }

        [Fact]
        public void When_TryConfigureWithNullKeyEqualityComparer_Then_ThrowException()
        {
            var sut = new InMemoryCacheConfigurations<FakeKey, FakeValue>.InMemoryCacheConfigurationsBuilder();

            Assert.Throws<ArgumentNullException>(() => sut.WithKeyEqualityComparer(null));
        }

        [Fact]
        public void When_TryConfigureWithValidKeyEqualityComparer_Then_BuildSuccessfully()
        {
            var sut = new InMemoryCacheConfigurations<FakeKey, FakeValue>.InMemoryCacheConfigurationsBuilder();
            sut.WithTTL(_ttl);

            sut.WithKeyEqualityComparer(FakeKeyEqualityComparer.Instance);

            var configs = sut.Build();

            configs.KeyEqualityComparer
                .Should().Be(FakeKeyEqualityComparer.Instance);
        }

        [Fact]
        public void When_TryConfigureWithNullValueEqualityComparer_Then_ThrowException()
        {
            var sut = new InMemoryCacheConfigurations<FakeKey, FakeValue>.InMemoryCacheConfigurationsBuilder();

            Assert.Throws<ArgumentNullException>(() => sut.ShareValuesBetweenKeys(null));
        }

        [Fact]
        public void When_TryConfigureWithValidValueEqualityComparer_Then_BuildSuccessfully()
        {
            var sut = new InMemoryCacheConfigurations<FakeKey, FakeValue>.InMemoryCacheConfigurationsBuilder();
            sut.WithTTL(_ttl);

            sut.ShareValuesBetweenKeys(FakeValueEqualityComparer.Instance);

            var configs = sut.Build();

            configs.ShareValuesBetweenKeys
                .Should().BeTrue();

            configs.ValueEqualityComparer
                .Should().Be(FakeValueEqualityComparer.Instance);
        }

        [Fact]
        public void When_TryConfigureWithDefautCleanerInterval_Then_ThrowException()
        {
            var sut = new InMemoryCacheConfigurations<FakeKey, FakeValue>.InMemoryCacheConfigurationsBuilder();

            Assert.Throws<ArgumentException>(() => sut.WithCleaner(default));
        }

        [Fact]
        public void When_TryConfigureWithValidCleanerInterval_Then_BuildSuccessfully()
        {
            var sut = new InMemoryCacheConfigurations<FakeKey, FakeValue>.InMemoryCacheConfigurationsBuilder();
            sut.WithTTL(_ttl);

            sut.WithCleaner(_cleanerInterval);

            var configs = sut.Build();

            configs.UseCleaner
                .Should().BeTrue();

            configs.CleaningInterval
                .Should().Be(_cleanerInterval);


            sut = new InMemoryCacheConfigurations<FakeKey, FakeValue>.InMemoryCacheConfigurationsBuilder();
            sut.WithTTL(_ttl);

            sut.WithCleaner(_cleanerInterval, _cleanerCheckInterval);

            configs = sut.Build();

            configs.UseCleaner
                .Should().BeTrue();

            configs.CleaningInterval
                .Should().Be(_cleanerInterval);

            configs.DelayToNextCancellationRequestedCheck
                .Should().Be(_cleanerCheckInterval);
        }

        [Fact]
        public void When_TryConfigureWithNullValueFactoryProxy_Then_ThrowException()
        {
            var sut = new InMemoryCacheConfigurations<FakeKey, FakeValue>.InMemoryCacheConfigurationsBuilder();

            Assert.Throws<ArgumentNullException>(() => sut.UsingValueFactoryProxy(default));
        }

        [Fact]
        public void When_TryConfigureWithValidValueFactoryProxy_Then_BuildSuccessfully()
        {
            var sut = new InMemoryCacheConfigurations<FakeKey, FakeValue>.InMemoryCacheConfigurationsBuilder();
            sut.WithTTL(_ttl);

            var moqValueFactoryProxy = Mock.Of<IValueFactoryProxy<FakeValue>>();

            sut.UsingValueFactoryProxy(moqValueFactoryProxy);

            var configs = sut.Build();

            configs.ValueFactoryProxy
                .Should().Be(moqValueFactoryProxy);
        }
    }
}
