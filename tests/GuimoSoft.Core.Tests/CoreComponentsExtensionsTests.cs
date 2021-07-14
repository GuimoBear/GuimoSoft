using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using GuimoSoft.Core.AspNetCore;
using GuimoSoft.Logger;
using GuimoSoft.Core.Providers.Interfaces;
using Xunit;

namespace GuimoSoft.Core.Tests
{
    public class CoreComponentsExtensionsTests
    {
        [Fact]
        public void When_CallAddApiLogger_Then_AddDependenciesInCollection()
        {
            var services = new ServiceCollection();
            services.AddCoreComponents();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IHttpContextAccessor) &&
                                          sd.Lifetime == ServiceLifetime.Singleton)
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IApiLoggerContextAccessor) &&
                                          sd.Lifetime == ServiceLifetime.Singleton)
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IApiLogger<>) &&
                                          sd.ImplementationType == typeof(ApiLogger<>) &&
                                          sd.Lifetime == ServiceLifetime.Singleton)
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IProviderExtension) &&
                                          sd.Lifetime == ServiceLifetime.Singleton &&
                                          sd.ImplementationFactory is not null)
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(CorrelationIdProvider) &&
                                          sd.Lifetime == ServiceLifetime.Scoped)
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(ICorrelationIdSetter) &&
                                          sd.Lifetime == ServiceLifetime.Scoped)
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(ICorrelationIdProvider) &&
                                          sd.Lifetime == ServiceLifetime.Scoped)
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(CorrelationId) &&
                                          sd.Lifetime == ServiceLifetime.Scoped)
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(TenantProvider) &&
                                          sd.Lifetime == ServiceLifetime.Scoped)
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(ITenantSetter) &&
                                          sd.Lifetime == ServiceLifetime.Scoped)
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(ITenantProvider) &&
                                          sd.Lifetime == ServiceLifetime.Scoped)
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(Tenant) &&
                                          sd.Lifetime == ServiceLifetime.Scoped)
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(CoreValueObjectsInitializerMiddleware) &&
                                          sd.Lifetime == ServiceLifetime.Singleton)
                .Should().NotBeNull();
        }

        [Fact]
        public void When_CallUseApiLoggerContextAccessor_Then_AddMiddlewareInApplicationBuilder()
        {
            var moqApplicationBuilder = new Mock<IApplicationBuilder>();
            moqApplicationBuilder
                .Setup(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
                .Verifiable();

            moqApplicationBuilder.Object.UseCoreComponents();

            moqApplicationBuilder
                .Verify(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Exactly(2));
        }
    }
}
