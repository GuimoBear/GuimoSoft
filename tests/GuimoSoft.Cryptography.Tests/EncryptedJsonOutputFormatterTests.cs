using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Cryptography.AspNetCore;
using GuimoSoft.Cryptography.AspNetCore.Formatters;
using GuimoSoft.Cryptography.RSA.Services.Interfaces;
using GuimoSoft.Cryptography.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Cryptography.Tests
{
    public class EncryptedJsonOutputFormatterTests
    {
        [Fact]
        public void When_CrypterServiceIsNull_Then_ConstructThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EncryptedJsonOutputFormatter(null, default));
        }

        [Fact]
        public void When_CrypterServiceIsValid_Then_ConstructAddSupportedMediaTypes()
        {
            var sut = new EncryptedJsonOutputFormatter(Mock.Of<ICrypterService>(), default);

            sut.SupportedMediaTypes
                .Should().Contain(MediaTypeHeaderValue.Parse(Constants.ENCRYPTED_CONTENT_TYPE).ToString());

            sut.SupportedMediaTypes
                .Should().Contain(MediaTypeHeaderValue.Parse(Constants.ENCRYPTED_BASE64_CONTENT_TYPE).ToString());
        }

        [Theory]
        [InlineData(Constants.ENCRYPTED_CONTENT_TYPE)]
        [InlineData(Constants.ENCRYPTED_BASE64_CONTENT_TYPE)]
        public async Task When_EncryptOutputContent_Then_FillContentIntoResponseBody(string contentType)
        {
            var (moqRequestHeaders, moqResponseHeaders, moqResponseBody, moqResponse, moqHttpContext) = GetHttpContext(contentType);
            var certId = Guid.NewGuid();

            var moqCrypterService = new Mock<ICrypterService>();
            moqCrypterService.Setup(x => x.Encrypt(It.IsAny<Guid>(), It.IsAny<byte[]>()))
                .ReturnsAsync(new byte[0]);

            var formatterContext = new OutputFormatterWriteContext(moqHttpContext.Object, (_, _) => Mock.Of<TextWriter>(), typeof(FakeRequest), new FakeRequest("t", 2));

            var sut = new EncryptedJsonOutputFormatter(moqCrypterService.Object, certId);

            await sut.WriteResponseBodyAsync(formatterContext);

            formatterContext.ContentType.Value
                .Should().Be(contentType);

            moqResponseHeaders
                .Verify(x => x.Add(Constants.RSA_IDENTIFIER_HEADER, certId.ToString()), Times.Once);

            moqResponse
                .VerifySet(x => x.ContentLength = 0);

            moqResponseBody
                .Verify(x => x.WriteAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        private static (Mock<IHeaderDictionary> moqRequestHeaders, Mock<IHeaderDictionary> moqResponseHeaders, Mock<Stream> moqResponseBody, Mock<HttpResponse> moqResponse, Mock<HttpContext> moqHttpContext) GetHttpContext(string contentType = Constants.ENCRYPTED_CONTENT_TYPE)
        {
            var request = new Mock<HttpRequest>();
            var headers = new Mock<IHeaderDictionary>();
            request.SetupGet(r => r.Headers).Returns(headers.Object);
            request.SetupGet(f => f.ContentType).Returns(contentType);

            var responseHeaders = new Mock<IHeaderDictionary>();
            responseHeaders
                .Setup(x => x.Add(It.IsAny<string>(), It.IsAny<StringValues>()))
                .Verifiable();

            var moqResponseBody = new Mock<Stream>();
            moqResponseBody
                .Setup(x => x.WriteAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var response = new Mock<HttpResponse>();
            response.SetupGet(r => r.Headers).Returns(responseHeaders.Object);
            response.SetupGet(r => r.Body).Returns(moqResponseBody.Object);
            response.SetupSet(r => r.ContentLength = It.IsAny<long?>()).Verifiable();

            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            httpContext.SetupGet(c => c.Response).Returns(response.Object);
            return (headers, responseHeaders, moqResponseBody, response, httpContext);
        }
    }
}
