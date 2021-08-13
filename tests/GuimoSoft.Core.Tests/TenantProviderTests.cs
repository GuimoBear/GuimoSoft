using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using GuimoSoft.Core.AspNetCore;
using GuimoSoft.Core.AspNetCore.Constants;
using GuimoSoft.Core.AspNetCore.Exceptions;
using Xunit;

namespace GuimoSoft.Core.Tests
{
    public class TenantProviderTests
    {
        private (Mock<IHttpContextAccessor>, TenantProvider) CriarProvider(string origem)
        {
            var moq = new Mock<IHttpContextAccessor>();
            moq.Setup(x => x.HttpContext.Request.Headers[RequestConstants.ORIGEM_HEADER]).Returns(origem);

            return (moq, new TenantProvider(moq.Object, Mock.Of<IProviderExtension>()));
        }
        private (Mock<IHttpContextAccessor>, TenantProvider) CriarProviderSemMoqPresetado()
        {
            var moq = new Mock<IHttpContextAccessor>();
            return (moq, new TenantProvider(moq.Object, Mock.Of<IProviderExtension>()));
        }

        private TenantProvider CriarProviderSemHeaderMasComProviderExtension(string origem)
        {
            var moqAccessor = new Mock<IHttpContextAccessor>();

            var moqProviderExtension = new Mock<IProviderExtension>();
            moqProviderExtension
                .Setup(x => x.GetTenant(It.IsAny<HttpContext>()))
                .ReturnsAsync(new Tenant(origem));

            return new TenantProvider(moqAccessor.Object, moqProviderExtension.Object);
        }

        private TenantProvider CriarProviderComAccessorNuloMasComProviderExtension(string origem)
        {
            var moqProviderExtension = new Mock<IProviderExtension>();
            moqProviderExtension
                .Setup(x => x.GetTenant(It.IsAny<HttpContext>()))
                .ReturnsAsync(new Tenant(origem));

            return new TenantProvider(null, moqProviderExtension.Object);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Se_HeaderOrigemSetadoMasTenantPreSetadoEhNuloOuVazio_Entao_TenantIsPontoFrio(string origem)
        {
            var (_, provider) = CriarProvider("teste");

            provider.SetTenant(origem);

            provider.Obter()
                .Should().Be("teste");
        }

        [Fact]
        public void ObterDeProviderExtensionsFacts()
        {
            var sut = CriarProviderSemHeaderMasComProviderExtension("teste");

            sut.Obter()
                .Should().Be("teste");

            sut = CriarProviderComAccessorNuloMasComProviderExtension("teste");

            sut.Obter()
                .Should().Be("teste");
        }

        [Fact]
        public void ObterWithoutAnyInnerProviderShouldReturnEmptyTenant()
        {
            var sut = new TenantProvider(null, Mock.Of<IProviderExtension>());

            sut.Obter()
                .Should().Be(new Tenant(""));
        }

        [Fact]
        public void ObterWithoutHttpContextAccessorShouldReturnEmptyTenant()
        {
            var (moqAccessor, provider) = CriarProviderSemMoqPresetado();

            moqAccessor
                .SetupGet(a => a.HttpContext)
                .Returns(default(HttpContext));

            provider.Obter()
                .Should().Be(new Tenant(""));
        }

        [Fact]
        public void ObterWithoutResponseShouldReturnEmptyTenant()
        {
            var (moqAccessor, provider) = CriarProviderSemMoqPresetado();

            moqAccessor
                .SetupGet(a => a.HttpContext.Response)
                .Returns(default(HttpResponse));

            provider.Obter()
                .Should().Be(new Tenant(""));
        }

        [Fact]
        public void ObterWithoutHeadersShouldReturnEmptyTenant()
        {
            var (moqAccessor, provider) = CriarProviderSemMoqPresetado();

            moqAccessor
                .SetupGet(a => a.HttpContext.Response.Headers)
                .Returns(default(IHeaderDictionary));

            provider.Obter()
                .Should().Be(new Tenant(""));
        }

        [Fact]
        public void ObterWithoutOrigemHeaderShouldReturnEmptyTenant()
        {
            var (moqAccessor, provider) = CriarProviderSemMoqPresetado();

            moqAccessor
                .Setup(x => x.HttpContext.Request.Headers[RequestConstants.ORIGEM_HEADER])
                .Returns("");

            provider.Obter()
                .Should().Be(new Tenant(""));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void SetTenantWithIgnoredDataFacts(string strTenant)
        {
            var (_, provider) = CriarProvider("teste");

            provider.SetTenant(strTenant);
            provider.SetTenant(strTenant);
        }

        [Fact]
        public void SetTenantWithSameTenantShouldNotThrowAnyException()
        {
            var (_, provider) = CriarProvider("teste");

            provider.SetTenant("teste");

            provider.SetTenant("teste");
        }

        [Fact]
        public void SetTenantWithDifferentTenantShouldThrowTenantJaSetadoException()
        {
            var (_, provider) = CriarProvider("teste");

            provider.SetTenant("teste");

            provider.SetTenant("teste");

            provider.SetTenant(null);

            Assert.Throws<TenantJaSetadoException>(() => provider.SetTenant("teste 2"));
        }

        [Fact]
        public void Se_HeaderOrigemEhTesteMasSetaTeste2_Entao_TenantIsTeste2()
        {
            var (_, provider) = CriarProvider("teste");

            provider.SetTenant("teste 2");

            provider.Obter()
                .Should().Be("teste 2");
        }

        [Fact]
        public void Se_TenantPreSetadoEhCasasBahiaETentarSetarNovamenteComExtra_Entao_ProviderEstouraTenantJaSetado()
        {
            var (_, provider) = CriarProvider("teste");

            provider.SetTenant("teste 2");

            Assert.Throws<TenantJaSetadoException>(() => provider.SetTenant("teste 3"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Se_HeaderOrigemEhNuloOuVazio_Entao_TenantIsInvalid(string origem)
        {
            var (_, provider) = CriarProvider(origem);
            Tenant tenant = provider.Obter();
            tenant.Equals(origem).Should().BeTrue();
        }
    }
}
