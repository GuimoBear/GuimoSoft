using FluentAssertions;
using Xunit;

namespace GuimoSoft.Core.Tests
{
    public class TenantTests
    {
        [Fact]
        public void Dado_UmTenantNaoNulo_Se_ComparadoComTenantNaoNulo_Entao_RetornaFalse()
        {
            Tenant tenant = "teste";
            Tenant another = "teste 2";

            tenant.Equals(another).Should().BeFalse();

            tenant.GetHashCode().Should().NotBe(another.GetHashCode());
        }

        [Fact]
        public void Dado_UmTenantNaoNulo_Se_ComparadoComTenantNulo_Entao_RetornaFalse()
        {
            Tenant tenant = "teste";
            Tenant another = null;

            tenant.Equals(another).Should().BeFalse();
        }

        [Fact]
        public void Dado_UmTenantNaoNulo_Se_ComparadoComStringNaoNulo_Entao_RetornaFalse()
        {
            Tenant tenant = "teste";
            string another = "teste 2";

            tenant.Equals(another).Should().BeFalse();
        }

        [Fact]
        public void Dado_UmTenantNaoNulo_Se_ComparadoComStringNula_Entao_RetornaFalse()
        {
            Tenant tenant = "teste";
            string another = null;

            tenant.Equals(another).Should().BeFalse();
        }

        [Fact]
        public void Dado_UmTenantTeste_Se_ConverterExplicitamenteParaString_Entao_RetornaStringTeste()
        {
            Tenant tenant = "teste";
            string stringTenant = tenant;

            stringTenant.Should().Be("teste");
        }

        [Fact]
        public void Se_TenantIsTeste_Entao_EqualComObjetoStringTeste_Entao_RetornaTrue()
        {
            Tenant tenant = "teste";
            object objectStringTenant = "teste";

            Assert.True(tenant.Equals(objectStringTenant));
        }

        [Fact]
        public void Dado_TenantNull_Quando_CastParaStringImplicito_Entao_RetornaStringNula()
        {
            Tenant tenant = null;
            string expected = tenant;

            expected
                .Should().BeEmpty();
        }

        [Fact]
        public void Dado_TenantIsString_Quando_EqualsComObject_Entao_RetornaFalse()
        {
            Tenant tenant = "teste";
            var anotherObject = new object();

            tenant.Equals(anotherObject)
                .Should().BeFalse();
        }

        [Fact]
        public void Dado_TenantIsString_Quando_EqualTenantNull_Entao_RetornaFalse()
        {
            Tenant tenant = "teste";
            Tenant another = null;

            tenant.Equals(another)
                .Should().BeFalse();
        }
    }
}
