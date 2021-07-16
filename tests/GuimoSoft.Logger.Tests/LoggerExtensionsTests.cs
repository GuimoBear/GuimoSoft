using FluentAssertions;
using GuimoSoft.Logger.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace GuimoSoft.Logger.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void When_CallAddApiLogger_Then_AddDependenciesInCollection()
        {
            var services = new ServiceCollection();
            services.AddApiLogger();

            services
                .Should().HaveCount(3);

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IApiLoggerContextAccessor) &&
                                          sd.ImplementationType == typeof(ApiLoggerContextAccessor) &&
                                          sd.Lifetime == ServiceLifetime.Singleton)
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(ApiLoggerAccessorInitializerMiddleware) &&
                                          sd.Lifetime == ServiceLifetime.Singleton)
                .Should().NotBeNull();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(IApiLogger<>) &&
                                          sd.ImplementationType == typeof(ApiLogger<>) &&
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

            moqApplicationBuilder.Object.UseApiLoggerContextAccessor();

            moqApplicationBuilder
                .Verify(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
        }
    }
}
