using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GuimoSoft.Core.Tests
{
    public class ProviderExtensionTests
    {
        private static readonly object _lock = new();

        [Fact]
        public void When_CallUseAdditionalTenantProviderWithNullProvider_Then_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ProviderExtension.UseAdditionalTenantProvider(default));
        }

        [Fact]
        public void When_CallUseAdditionalCorrelationIdProviderWithNullProvider_Then_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ProviderExtension.UseAdditionalCorrelationIdProvider(default));
        }

        [Fact]
        public void Given_NoRegisteredAdictionalProvider_When_CallGetTenant_Then_ReturnDefaultTenant()
        {
            lock (_lock)
            {
                var sut = new ProviderExtension();

                var moqHttpContext = new Mock<HttpContext>();

                var result = sut.GetTenant(moqHttpContext.Object).ConfigureAwait(false).GetAwaiter().GetResult();

                result
                    .Should().Be(default(Tenant));

                ProviderExtension.additionalTenantProvidersSingleton.Clear();
            }
        }

        [Fact]
        public void Given_RegisteredAdictionalProviderReturningAnNullTenant_When_CallGetTenant_Then_ReturnDefaultTenant()
        {
            lock (_lock)
            {
                ProviderExtension.UseAdditionalTenantProvider(_ => Task.FromResult<Tenant>(default));

                var sut = new ProviderExtension();

                var moqHttpContext = new Mock<HttpContext>();

                var result = sut.GetTenant(moqHttpContext.Object).ConfigureAwait(false).GetAwaiter().GetResult();

                result
                    .Should().Be(default(Tenant));

                ProviderExtension.additionalTenantProvidersSingleton.Clear();
            }
        }

        [Fact]
        public void When_CallGetTenantWithRegisteredAddictionalProvider_Then_ReturnTenant()
        {
            lock (_lock)
            {
                var moqHttpContext = new Mock<HttpContext>();
                moqHttpContext
                    .Setup(x => x.Request.Path)
                    .Returns("/teste");

                ProviderExtension.UseAdditionalTenantProvider(GetTenantFromRequest);

                var sut = new ProviderExtension();

                sut.GetTenant(moqHttpContext.Object).ConfigureAwait(false).GetAwaiter().GetResult();

                moqHttpContext.Verify(x => x.Request.Path, Times.Once);

                ProviderExtension.additionalTenantProvidersSingleton.Clear();
            }
        }

        [Fact]
        public void Given_NoRegisteredAdictionalProvider_When_CallGetCorrelationId_Then_ReturnDefaultTenant()
        {
            lock (_lock)
            {
                var sut = new ProviderExtension();

                var moqHttpContext = new Mock<HttpContext>();

                var result = sut.GetCorrelationId(moqHttpContext.Object).ConfigureAwait(false).GetAwaiter().GetResult();

                result
                    .Should().Be(default(CorrelationId));

                ProviderExtension.additionalCorrelationIdProvidersSingleton.Clear();
            }
        }

        [Fact]
        public void Given_RegisteredAdictionalProviderReturningAnNullCorrelationId_When_CallGetCorrelationId_Then_ReturnDefaultTenant()
        {
            lock (_lock)
            {
                ProviderExtension.UseAdditionalCorrelationIdProvider(_ => Task.FromResult<CorrelationId>(default));

                var sut = new ProviderExtension();

                var moqHttpContext = new Mock<HttpContext>();

                var result = sut.GetCorrelationId(moqHttpContext.Object).ConfigureAwait(false).GetAwaiter().GetResult();

                result
                    .Should().Be(default(CorrelationId));

                ProviderExtension.additionalCorrelationIdProvidersSingleton.Clear();
            }
        }

        [Fact]
        public void When_CallGetCorrelationIdWithRegisteredAddictionalProvider_Then_ReturnTenant()
        {
            lock (_lock)
            {
                var moqHttpContext = new Mock<HttpContext>();
                moqHttpContext
                    .Setup(x => x.Request.Path)
                    .Returns("/teste");

                ProviderExtension.UseAdditionalCorrelationIdProvider(GetCorrelationIdFromRequest);

                var sut = new ProviderExtension();

                sut.GetCorrelationId(moqHttpContext.Object).ConfigureAwait(false).GetAwaiter().GetResult();

                moqHttpContext.Verify(x => x.Request.Path, Times.Once);

                ProviderExtension.additionalCorrelationIdProvidersSingleton.Clear();
            }
        }

        private static Task<Tenant> GetTenantFromRequest(HttpContext context)
        {
            return Task.FromResult<Tenant>(context?.Request?.Path.ToString());
        }

        private static Task<CorrelationId> GetCorrelationIdFromRequest(HttpContext context)
        {
            return Task.FromResult<CorrelationId>(context?.Request?.Path.ToString());
        }
    }
}
