using FluentAssertions;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using GuimoSoft.Cache.InMemory;
using GuimoSoft.Cache.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Cache.Tests.InMemory
{
    public class InMemoryTypedCacheTests
    {
        [Fact]
        public void When_TryCreateWithConfigurerThrowException_Then_ThrowException()
        {
            Assert.Throws<Exception>(() => new TypedCache<FakeKey, FakeValue>(cfg => throw new Exception()));
        }

        [Fact]
        public async Task When_GetCachedValueOrAdd_Then_ReturnValue()
        {
            using var sut = new TypedCache<FakeKey, FakeValue>(cfg =>
            {
                cfg.WithTTL(TimeSpan.FromMilliseconds(100))
                   .WithKeyEqualityComparer(FakeKeyEqualityComparer.Instance)
                   .WithCleaner(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(10));
            });

            var key = new FakeKey("test", 5);
            var value = new FakeValue("test", 5);

            var counter = new ConcurrentBag<int>();

            FakeValue cachedValue = sut.Get(key)
                .OrAdd(() => value);

            ReferenceEquals(cachedValue, value)
                .Should().BeTrue(); 
            
            cachedValue = sut.Get(key)
                 .OrAdd(() => new FakeValue("test", 5));

            ReferenceEquals(cachedValue, value)
                .Should().BeTrue();

            await Task.Delay(200);

            cachedValue = sut.Get(key)
                 .OrAdd(() => new FakeValue("test", 5));

            ReferenceEquals(cachedValue, value)
                .Should().BeFalse();
        }

        [Fact]
        public async Task When_GetCachedValueOrAddAsync_Then_ReturnValue()
        {
            using var sut = new TypedCache<FakeKey, FakeValue>(cfg =>
            {
                cfg.WithTTL(TimeSpan.FromMilliseconds(100))
                   .WithKeyEqualityComparer(FakeKeyEqualityComparer.Instance)
                   .WithCleaner(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(20));
            });

            var key = new FakeKey("test", 5);
            var value = new FakeValue("test", 5);

            var counter = new ConcurrentBag<int>();

            FakeValue cachedValue = await sut.Get(key)
                .OrAddAsync(() => Task.FromResult(value));

            ReferenceEquals(cachedValue, value)
                .Should().BeTrue();

            cachedValue = cachedValue = await sut.Get(key)
                .OrAddAsync(() => Task.FromResult(new FakeValue("test", 5)));

            ReferenceEquals(cachedValue, value)
                .Should().BeTrue();

            await Task.Delay(200);

            cachedValue = cachedValue = await sut.Get(key)
                .OrAddAsync(() => Task.FromResult(new FakeValue("test", 5)));

            ReferenceEquals(cachedValue, value)
                .Should().BeFalse();
        }

        [Fact]
        public void When_GetCachedValueOrAddWithSharedValueBetweenKeys_Then_ReturnValue()
        {
            using var sut = new TypedCache<FakeKey, FakeValue>(cfg =>
            {
                cfg.WithTTL(TimeSpan.FromMinutes(30))
                   .WithKeyEqualityComparer(FakeKeyEqualityComparer.Instance)
                   .ShareValuesBetweenKeys(FakeValueEqualityComparer.Instance)
                   .WithCleaner(TimeSpan.FromSeconds(120), TimeSpan.FromMilliseconds(500));
            });

            var key1 = new FakeKey("test", 5);
            var key2 = new FakeKey("test 2", 8);
            var value1 = new FakeValue("test", 5);
            var value2 = new FakeValue("test", 5);

            var counter = new ConcurrentBag<int>();

            FakeValue cachedValue = sut.Get(key1)
                .OrAdd(() => value1);

            ReferenceEquals(cachedValue, value1)
                .Should().BeTrue();

            cachedValue = sut.Get(key2)
                .OrAdd(() => value2);

            ReferenceEquals(cachedValue, value1)
                .Should().BeTrue();
        }
    }
}
