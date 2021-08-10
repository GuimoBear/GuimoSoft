using FluentAssertions;
using System;
using Xunit;

namespace GuimoSoft.Core.Tests
{
    public class CorrelationIdTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Se_ValorVazioOuNulo_Entao_Invalido(string strCorrelationId)
        {
            CorrelationId correlationId = strCorrelationId;
            Assert.False(string.IsNullOrEmpty(correlationId.Value));
        }

        [Fact]
        public void Se_ValorNaoEhNuloOuVazio_Entao_Valido()
        {
            string expectedCorrelationId = Guid.NewGuid().ToString();
            CorrelationId correlationId = expectedCorrelationId;
            Assert.Equal(expectedCorrelationId, correlationId.Value);
        }

        [Fact]
        public void Se_CorrelationIsTeste_Entao_GetHashCodeRetornaHetHashCodeDeTeste()
        {
            var correlationIdTeste = "teste";
            CorrelationId correlationId = correlationIdTeste;

            correlationId.GetHashCode().Should().Be(correlationIdTeste.GetHashCode());
            Assert.True(correlationId == correlationIdTeste);
            Assert.False(correlationId != correlationIdTeste);
            Assert.True(correlationId.Equals(correlationIdTeste));
        }

        [Fact]
        public void Dado_UmCorrelationIdNulo_Se_ComparadoComCorrelationIdNaoNulo_Entao_RetornaFalse()
        {
            CorrelationId correlationId = "teste";
            CorrelationId another = null;

            correlationId.Equals(another).Should().BeFalse();
            Assert.False(correlationId == another);
            Assert.True(correlationId != another);
            Assert.False(correlationId.Equals(string.Empty));
        }

        [Fact]
        public void Dado_UmCorrelationIdNulo_Se_ComparadoComOutroCorrelationIdNulo_Entao_RetornaTrue()
        {
            CorrelationId correlationId = null;
            CorrelationId another = null;

            Assert.True(correlationId == another);
            Assert.False(correlationId != another);
        }

        [Fact]
        public void Se_CorrelationIsTeste_Entao_EqualComObjetoStringTeste_Entao_RetornaTrue()
        {
            CorrelationId correlationId = "teste";
            object objectStringCorrelationId = "teste";

            Assert.True(correlationId.Equals(objectStringCorrelationId));
            Assert.Equal(correlationId.GetHashCode(), correlationId.GetHashCode()); 
            
            objectStringCorrelationId = null;

            Assert.False(correlationId.Equals(objectStringCorrelationId));
        }

        [Fact]
        public void Se_CorrelationIsTeste_Entao_EqualComCorrelationIdTeste_Entao_RetornaTrue()
        {
            CorrelationId correlationId = "teste";
            CorrelationId objectStringCorrelationId = "teste";

            Assert.True(correlationId.Equals((object)objectStringCorrelationId));
        }

        [Fact]
        public void Se_CorrelationIsTeste_Entao_EqualComObject_Entao_RetornaFalse()
        {
            CorrelationId correlationId = "teste";
            var anotherObject = new object();

            Assert.False(correlationId.Equals(anotherObject));
        }

        [Fact]
        public void Se_CorrelationIsTeste_Entao_EqualComStringNula_Entao_RetornaFalse()
        {
            CorrelationId correlationId = "teste";
            string nullString = null;

            Assert.False(correlationId.Equals(nullString));
        }
    }
}
