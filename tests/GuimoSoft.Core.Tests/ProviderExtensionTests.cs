using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GuimoSoft.Core.Tests
{
    public class ProviderExtensionTests
    {
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
        public async Task When_CallGetTenantWithRegisteredAddictionalProvider_Then_ReturnTenant()
        {
            var moqHttpContext = new Mock<HttpContext>();
            moqHttpContext
                .Setup(x => x.Request.Path)
                .Returns("/teste");

            ProviderExtension.UseAdditionalTenantProvider(GetTenantFromRequest);

            var sut = new ProviderExtension();

            await sut.GetTenant(moqHttpContext.Object);

            moqHttpContext.Verify(x => x.Request.Path, Times.Once);
        }

        [Fact]
        public async Task When_CallGetCorrelationIdWithRegisteredAddictionalProvider_Then_ReturnTenant()
        {
            var moqHttpContext = new Mock<HttpContext>();
            moqHttpContext
                .Setup(x => x.Request.Path)
                .Returns("/teste");

            ProviderExtension.UseAdditionalCorrelationIdProvider(GetCorrelationIdFromRequest);

            var sut = new ProviderExtension();

            await sut.GetCorrelationId(moqHttpContext.Object);

            moqHttpContext.Verify(x => x.Request.Path, Times.Once);
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
