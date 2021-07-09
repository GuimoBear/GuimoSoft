using FluentAssertions;
using Moq;
using System;
using GuimoSoft.Cache.Exceptions;
using GuimoSoft.Cache.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Cache.Tests
{
    public class CacheTests
    {
        [Fact]
        public void Dado_UmServiceProviderNulo_Se_Construir_Entao_EstouraArgumentnullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Cache(default));
        }

        [Fact]
        public void Dado_UmServiceProviderSemOCache_Se_Obter_Entao_EstouraCacheNotConfiguredException()
        {
            var moqServiceProvider = new Mock<IServiceProvider>();
            moqServiceProvider
                .Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(null);

            var sut = new Cache(moqServiceProvider.Object);

            Assert.Throws<CacheNotConfiguredException>(() => sut.Get<FakeKey, FakeValue>(new FakeKey("test", 5)));

            moqServiceProvider
                .Verify(x => x.GetService(typeof(ITypedCache<FakeKey, FakeValue>)), Times.Once);
        }

        [Fact]
        public void Dado_UmServiceProviderComOCache_Se_Obter_Entao_RetornaBuilder()
        {
            var key = new FakeKey("test", 5);

            var cacheBuilder = new CacheItemBuilder<FakeKey, FakeValue>(key, default, default, Mock.Of<IValueFactoryProxy<FakeValue>>());

            var moqTypedCache = new Mock<ITypedCache<FakeKey, FakeValue>>();
            moqTypedCache
                .Setup(x => x.Get(It.IsAny<FakeKey>()))
                .Returns(cacheBuilder);

            var moqServiceProvider = new Mock<IServiceProvider>();
            moqServiceProvider
                .Setup(x => x.GetService(typeof(ITypedCache<FakeKey, FakeValue>)))
                .Returns(() => moqTypedCache.Object);

            var sut = new Cache(moqServiceProvider.Object);

            var builder = sut.Get<FakeKey, FakeValue>(key);

            ReferenceEquals(builder, cacheBuilder)
                .Should().BeTrue();

            moqServiceProvider
                .Verify(x => x.GetService(typeof(ITypedCache<FakeKey, FakeValue>)), Times.Once);

            moqTypedCache
                .Verify(x => x.Get(key), Times.Once);
        }
    }
}
