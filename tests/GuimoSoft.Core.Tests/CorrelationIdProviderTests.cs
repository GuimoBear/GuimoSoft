using FluentAssertions;
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
        public void SetCorrelationIdInResponseHeaderWithoutAccessorShouldNotSetHeader()
        {
            var provider = new CorrelationIdProvider(null, Mock.Of<IProviderExtension>());

            provider.SetCorrelationIdInResponseHeader();
        }

        [Fact]
        public void ObterWithoutAnyInnerProviderShouldReturnEmptyCorrelationId()
        {
            var sut = new CorrelationIdProvider(null, Mock.Of<IProviderExtension>());

            sut.Get()
                .Should().NotBeNull();
        }

        [Fact]
        public void ObterWithoutHttpContextAccessorShouldReturnEmptyCorrelationId()
        {
            var (moqAccessor, provider) = CriarProviderSemMoqPresetado();

            moqAccessor
                .SetupGet(a => a.HttpContext)
                .Returns(default(HttpContext));

            provider.Get()
                .Should().NotBeNull();
        }

        [Fact]
        public void ObterWithoutResponseShouldReturnEmptyCorrelationId()
        {
            var (moqAccessor, provider) = CriarProviderSemMoqPresetado();

            moqAccessor
                .SetupGet(a => a.HttpContext.Response)
                .Returns(default(HttpResponse));

            provider.Get()
                .Should().NotBeNull();
        }

        [Fact]
        public void ObterWithoutHeadersShouldReturnEmptyCorrelationId()
        {
            var (moqAccessor, provider) = CriarProviderSemMoqPresetado();

            moqAccessor
                .SetupGet(a => a.HttpContext.Response.Headers)
                .Returns(default(IHeaderDictionary));

            provider.Get()
                .Should().NotBeNull();
        }

        [Fact]
        public void ObterWithoutOrigemHeaderShouldReturnEmptyCorrelationId()
        {
            var (moqAccessor, provider) = CriarProviderSemMoqPresetado();

            moqAccessor
                .Setup(x => x.HttpContext.Request.Headers[RequestConstants.ORIGEM_HEADER])
                .Returns("");

            provider.Get()
                .Should().NotBeNull();
        }

        [Fact]
        public void SetCorrelationIdInResponseHeaderWithoutHttpContextShouldNotSetHeader()
        {
            var expectedCorrelationId = Guid.NewGuid().ToString();
            var (accessorMock, provider) = CriarProvider(expectedCorrelationId);

            accessorMock
                .SetupGet(a => a.HttpContext)
                .Returns(default(HttpContext));

            provider.SetCorrelationIdInResponseHeader();

            accessorMock.Verify(a => a.HttpContext.Response.Headers.Add(
                RequestConstants.CORRELATION_ID_HEADER,
                expectedCorrelationId
            ), Times.Never);
        }

        [Fact]
        public void SetCorrelationIdInResponseHeaderWithoutResponseShouldNotSetHeader()
        {
            var expectedCorrelationId = Guid.NewGuid().ToString();
            var (accessorMock, provider) = CriarProvider(expectedCorrelationId);

            accessorMock
                .SetupGet(a => a.HttpContext.Response)
                .Returns(default(HttpResponse));

            provider.SetCorrelationIdInResponseHeader();

            accessorMock.Verify(a => a.HttpContext.Response.Headers.Add(
                RequestConstants.CORRELATION_ID_HEADER,
                expectedCorrelationId
            ), Times.Never);
        }

        [Fact]
        public void SetCorrelationIdInResponseHeaderWithoutHeadersShouldNotSetHeader()
        {
            var expectedCorrelationId = Guid.NewGuid().ToString();
            var (accessorMock, provider) = CriarProvider(expectedCorrelationId);

            accessorMock
                .SetupGet(a => a.HttpContext.Response.Headers)
                .Returns(default(IHeaderDictionary));

            provider.SetCorrelationIdInResponseHeader();

            accessorMock.Verify(a => a.HttpContext.Response.Headers.Add(
                RequestConstants.CORRELATION_ID_HEADER,
                expectedCorrelationId
            ), Times.Never);
        }

        [Fact]
        public void SetCorrelationIdInResponseHeaderShouldSetHeader()
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
        private (Mock<IHttpContextAccessor>, CorrelationIdProvider) CriarProviderSemMoqPresetado()
        {
            var moq = new Mock<IHttpContextAccessor>();
            return (moq, new CorrelationIdProvider(moq.Object, Mock.Of<IProviderExtension>()));
        }
    }
}
