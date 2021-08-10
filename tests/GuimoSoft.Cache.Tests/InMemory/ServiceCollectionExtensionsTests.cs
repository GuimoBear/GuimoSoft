using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using GuimoSoft.Cache.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Cache.Tests.InMemory
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddInMemoryCacheFacts()
        {
            var services = new ServiceCollection();

            Assert.Throws<Exception>(() => services.AddInMemoryCache<FakeKey, FakeValue>(_ => throw new Exception()));

            services
                .Should().BeEmpty();

            services.AddInMemoryCache<FakeKey, FakeValue>(configs => configs.WithTTL(TimeSpan.FromSeconds(60)));

            services.FirstOrDefault(sd => sd.ServiceType == typeof(ITypedCache<FakeKey, FakeValue>) &&
                                          sd.Lifetime == ServiceLifetime.Singleton && 
                                          sd.ImplementationInstance is not null)
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(ICache) &&
                                          sd.ImplementationType == typeof(Cache) &&
                                          sd.Lifetime == ServiceLifetime.Transient)
                .Should().NotBeNull();
        }
    }
}
