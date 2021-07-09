using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using GuimoSoft.AspNetCore;
using GuimoSoft.AspNetCore.Constants;
using GuimoSoft.AspNetCore.Exceptions;
using Xunit;

namespace GuimoSoft.Tests
{
    public class TenantProviderTests
    {
        private TenantProvider CriarProvider(string origem)
        {
            var moq = new Mock<IHttpContextAccessor>();
            moq.Setup(x => x.HttpContext.Request.Headers[RequestConstants.ORIGEM_HEADER]).Returns(origem);

            return new TenantProvider(moq.Object);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Se_HeaderOrigemSetadoMasTenantPreSetadoEhNuloOuVazio_Entao_TenantIsPontoFrio(string origem)
        {
            var provider = CriarProvider("teste");

            provider.SetTenant(origem);

            provider.Obter()
                .Should().Be("teste");
        }

        [Fact]
        public void Se_HeaderOrigemEhTesteMasSetaTeste2_Entao_TenantIsTeste2()
        {
            var provider = CriarProvider("teste");

            provider.SetTenant("teste 2");

            provider.Obter()
                .Should().Be("teste 2");
        }

        [Fact]
        public void Se_TenantPreSetadoEhCasasBahiaETentarSetarNovamenteComExtra_Entao_ProviderEstouraTenantJaSetado()
        {
            var provider = CriarProvider("teste");

            provider.SetTenant("teste 2");

            Assert.Throws<TenantJaSetadoException>(() => provider.SetTenant("teste 2"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Se_HeaderOrigemEhNuloOuVazio_Entao_TenantIsInvalid(string origem)
        {
            Tenant tenant = CriarProvider(origem).Obter();
            tenant.Equals(origem).Should().BeTrue();
        }
    }
}
