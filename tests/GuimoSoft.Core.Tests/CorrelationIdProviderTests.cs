using Microsoft.AspNetCore.Http;
using Moq;
using System;
using GuimoSoft.Core.AspNetCore;
using GuimoSoft.Core.AspNetCore.Constants;
using GuimoSoft.Core.AspNetCore.Exceptions;
using Xunit;

namespace GuimoSoft.Core.Tests
{
    public class CorrelationIdProviderTests
    {
        private (Mock<IHttpContextAccessor>, CorrelationIdProvider) CriarProvider(string correlationId)
        {
            var moq = new Mock<IHttpContextAccessor>();
            moq.Setup(x => x.HttpContext.Request.Headers[RequestConstants.CORRELATION_ID_HEADER]).Returns(correlationId);
            moq.Setup(x => x.HttpContext.Response.Headers[RequestConstants.CORRELATION_ID_HEADER]);

            var moqProviderExtension = new Mock<IProviderExtension>();
            moqProviderExtension
                .Setup(x => x.GetCorrelationId(It.IsAny<HttpContext>()))
                .ReturnsAsync(correlationId);

            return (moq, new CorrelationIdProvider(moq.Object, moqProviderExtension.Object));
        }

        [Fact]
        public void Se_HeaderCorrelationIdNaoEhNuloOuVazio_Entao_CorrelationIdIsNotEmpty()
        {
            var expectedCorrelationId = Guid.NewGuid().ToString();
            (var _, var provider) = CriarProvider(expectedCorrelationId);
            CorrelationId correlationId = provider.Get();
            Assert.Equal(expectedCorrelationId, correlationId.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Se_HeaderCorrelationIdEhNuloOuVazio_Entao_CorrelationIdIsEmpty(string strCorrelationId)
        {
            (var _, var provider) = CriarProvider(strCorrelationId);
            CorrelationId correlationId = provider.Get();
            Assert.False(string.IsNullOrEmpty(correlationId.Value));
        }

        [Fact]
        public void Se_HeaderCorrelationIdNaoEhNuloOuVazio_Entao_SetHeaderIsSameFromRequest()
        {
            var expectedCorrelationId = Guid.NewGuid().ToString();
            (var accessorMock, var provider) = CriarProvider(expectedCorrelationId);
            provider.SetCorrelationIdInResponseHeader();

            accessorMock.Verify(a => a.HttpContext.Response.Headers.Add(
                RequestConstants.CORRELATION_ID_HEADER,
                expectedCorrelationId
            ), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Se_HeaderCorrelationIdEhNuloOuVazio_Entao_SetHeaderIsRandomGuid(string strCorrelationId)
        {
            (var accessorMock, var provider) = CriarProvider(strCorrelationId);
            provider.SetCorrelationIdInResponseHeader();

            var correlationId = provider.Get().ToString();

            accessorMock.Verify(a => a.HttpContext.Response.Headers.Add(
                RequestConstants.CORRELATION_ID_HEADER,
                correlationId
            ), Times.Once);
        }



        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Se_HeaderCorrelationIdMasCorrelationIdPreSetadoEhNuloOuVazio_Entao_CorrelationIdEhValorNoHeader(string strCorrelationId)
        {
            var randomCorrelationId = Guid.NewGuid().ToString();
            (_, var provider) = CriarProvider(randomCorrelationId);

            provider.SetCorrelationId(strCorrelationId);

            Assert.Equal(provider.CorrelationId, provider.Get());
        }

        [Fact]
        public void Se_HeaderCorrelationIdMasCorrelationIdPreSetado_Entao_CorrelationIdIsPreSetado()
        {
            var randomCorrelationId = Guid.NewGuid().ToString();
            (_, var provider) = CriarProvider(randomCorrelationId);

            var newRandomCorrelationId = Guid.NewGuid().ToString();

            provider.SetCorrelationId(newRandomCorrelationId);

            Assert.Equal(newRandomCorrelationId, provider.Get());
        }

        [Fact]
        public void Se_CorrelationIdPreSetadoETentarSetarNovamente_Entao_ProviderEstouraCorrelationIdJaSetado()
        {
            var randomCorrelationId = Guid.NewGuid().ToString();
            (_, var provider) = CriarProvider(randomCorrelationId);

            provider.SetCorrelationId(Guid.NewGuid().ToString());

            var newRandomCorrelationId = Guid.NewGuid().ToString();

            Assert.Throws<CorrelationIdJaSetadoException>(() => provider.SetCorrelationId(Guid.NewGuid().ToString()));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Se_CorrelationIdPreSetadoEhNuloOuVazioETentarSetarNovamenteComRandom_Entao_CorrelationIdIsRandom(string strCorrelationId)
        {
            var randomCorrelationId = Guid.NewGuid().ToString();
            (_, var provider) = CriarProvider(randomCorrelationId);

            provider.SetCorrelationId(strCorrelationId);

            var newRandomCorrelationId = Guid.NewGuid().ToString();

            Assert.Throws<CorrelationIdJaSetadoException>(() => provider.SetCorrelationId(newRandomCorrelationId));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Se_CorrelationIdPreSetadoRandomETentarSetarNovamenteComEhNuloOuVazio_Entao_ProviderEstouraCorrelationIdJaSetado(string strCorrelationId)
        {
            var randomCorrelationId = Guid.NewGuid().ToString();
            (_, var provider) = CriarProvider(randomCorrelationId);

            var newRandomCorrelationId = Guid.NewGuid().ToString();

            provider.SetCorrelationId(newRandomCorrelationId);

            Assert.Throws<CorrelationIdJaSetadoException>(() => provider.SetCorrelationId(strCorrelationId));
        }
    }
}
