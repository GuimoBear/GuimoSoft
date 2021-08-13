using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GuimoSoft.Cryptography.AspNetCore;
using GuimoSoft.Cryptography.AspNetCore.Formatters;
using GuimoSoft.Cryptography.RSA.Services.Interfaces;
using GuimoSoft.Cryptography.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Cryptography.Tests
{
    public class EncryptedJsonInputFormatterTests
    {
        [Fact]
        public async Task RequestWithoutCertIdHeader_Return_FailureInputFormatterResult()
        {
            var request = new FakeRequest("test", 2);
            var stringContent = JsonConvert.SerializeObject(request);
            var contentBytes = Encoding.UTF8.GetBytes(stringContent);

            var (moqHeaders, moqHttpContext) = GetHttpContext(contentBytes);
            var modelMetadata = GetModelMetadata(request.GetType());

            StringValues certId = Guid.NewGuid().ToString();
            moqHeaders.Setup(x => x.TryGetValue(Constants.RSA_IDENTIFIER_HEADER, out certId))
                .Returns(false);

            var inputFormatterContext = new InputFormatterContext(moqHttpContext.Object, typeof(FakeRequest).FullName, new ModelStateDictionary(), modelMetadata, (stream, encoding) => new StreamReader(stream, encoding));

            var sut = new EncryptedJsonInputFormatter(Mock.Of<ICrypterService>());

            var result = await sut.ReadRequestBodyAsync(inputFormatterContext);

            result
                .Should().BeEquivalentTo(InputFormatterResult.Failure());

            moqHeaders.Verify(x => x.TryGetValue(Constants.RSA_IDENTIFIER_HEADER, out certId), Times.Once);
        }

        [Fact]
        [Obsolete]
        public async Task RequestWithCertIdHeaderAndInvalidGuid_Return_FailureInputFormatterResult()
        {
            var request = new FakeRequest("test", 2);
            var stringContent = JsonConvert.SerializeObject(request);
            var contentBytes = Encoding.UTF8.GetBytes(stringContent);

            var (moqHeaders, moqHttpContext) = GetHttpContext(contentBytes);
            var modelMetadata = GetModelMetadata(request.GetType());

            StringValues certId = Guid.NewGuid().ToString() + "x";
            moqHeaders.Setup(x => x.TryGetValue(Constants.RSA_IDENTIFIER_HEADER, out certId))
                .Returns(true);

            var inputFormatterContext = new InputFormatterContext(moqHttpContext.Object, typeof(FakeRequest).FullName, new ModelStateDictionary(), modelMetadata, (stream, encoding) => new StreamReader(stream, encoding));

            var sut = new EncryptedJsonInputFormatter(Mock.Of<ICrypterService>());

            var result = await sut.ReadRequestBodyAsync(inputFormatterContext);

            result
                .Should().BeEquivalentTo(InputFormatterResult.Failure());

            moqHeaders.Verify(x => x.TryGetValue(Constants.RSA_IDENTIFIER_HEADER, out certId), Times.Once);
        }


        [Fact]
        public async Task RequestWithInvalidEncryptedJson_Return_SuccessfulInputFormatterResult()
        {
            var request = new FakeRequest("test", 2);
            var stringContent = JsonConvert.SerializeObject(request).Substring(0, 15);
            var contentBytes = Encoding.UTF8.GetBytes(stringContent);

            var (moqHeaders, moqHttpContext) = GetHttpContext(contentBytes);
            var modelMetadata = GetModelMetadata(request.GetType());

            StringValues certId = Guid.NewGuid().ToString();
            moqHeaders.Setup(x => x.TryGetValue(Constants.RSA_IDENTIFIER_HEADER, out certId))
                .Returns(true);

            var inputFormatterContext = new InputFormatterContext(moqHttpContext.Object, typeof(FakeRequest).FullName, new ModelStateDictionary(), modelMetadata, (stream, encoding) => new StreamReader(stream, encoding));

            var moqCrypterService = new Mock<ICrypterService>();
            moqCrypterService.Setup(x => x.Decrypt(It.IsAny<Guid>(), It.IsAny<Stream>()))
                .ReturnsAsync(contentBytes);

            var sut = new EncryptedJsonInputFormatter(moqCrypterService.Object);

            var result = await sut.ReadRequestBodyAsync(inputFormatterContext);

            result
                .Should().BeEquivalentTo(InputFormatterResult.Success(null));

            moqHeaders.Verify(x => x.TryGetValue(Constants.RSA_IDENTIFIER_HEADER, out certId), Times.Once);

            moqCrypterService.Verify(x => x.Decrypt(It.IsAny<Guid>(), It.IsAny<Stream>()), Times.Once);
        }


        [Fact]
        public async Task RequestWithCertIdHeaderAndValidGuid_Return_SuccessInputFormatterResult()
        {
            var request = new FakeRequest("test", 2);
            var stringContent = JsonConvert.SerializeObject(request);
            var contentBytes = Encoding.UTF8.GetBytes(stringContent);

            var (moqHeaders, moqHttpContext) = GetHttpContext(contentBytes);
            var modelMetadata = GetModelMetadata(request.GetType());

            StringValues certId = Guid.NewGuid().ToString();
            moqHeaders.Setup(x => x.TryGetValue(Constants.RSA_IDENTIFIER_HEADER, out certId))
                .Returns(true);

            var inputFormatterContext = new InputFormatterContext(moqHttpContext.Object, typeof(FakeRequest).FullName, new ModelStateDictionary(), modelMetadata, (stream, encoding) => new StreamReader(stream, encoding));

            var moqCrypterService = new Mock<ICrypterService>();
            moqCrypterService.Setup(x => x.Decrypt(It.IsAny<Guid>(), It.IsAny<Stream>()))
                .ReturnsAsync(contentBytes);

            var sut = new EncryptedJsonInputFormatter(moqCrypterService.Object);

            var result = await sut.ReadRequestBodyAsync(inputFormatterContext);

            result
                .Should().BeEquivalentTo(InputFormatterResult.Success(request));

            moqHeaders.Verify(x => x.TryGetValue(Constants.RSA_IDENTIFIER_HEADER, out certId), Times.Once);

            moqCrypterService.Verify(x => x.Decrypt(It.IsAny<Guid>(), It.IsAny<Stream>()), Times.Once);
        }

        [Fact]
        public async Task RequestWithCertIdHeaderAndValidGuidAndBase64Content_Return_SuccessInputFormatterResult()
        {
            var request = new FakeRequest("test", 2);
            var stringContent = JsonConvert.SerializeObject(request);
            var contentBytes = Encoding.UTF8.GetBytes(stringContent);
            var base64Content = Convert.ToBase64String(contentBytes);

            var (moqHeaders, moqHttpContext) = GetHttpContext(Encoding.UTF8.GetBytes(base64Content), Constants.ENCRYPTED_BASE64_CONTENT_TYPE);
            var modelMetadata = GetModelMetadata(request.GetType());

            StringValues certId = Guid.NewGuid().ToString();
            moqHeaders.Setup(x => x.TryGetValue(Constants.RSA_IDENTIFIER_HEADER, out certId))
                .Returns(true);

            var inputFormatterContext = new InputFormatterContext(moqHttpContext.Object, typeof(FakeRequest).FullName, new ModelStateDictionary(), modelMetadata, (stream, encoding) => new StreamReader(stream, encoding));

            var moqCrypterService = new Mock<ICrypterService>();
            moqCrypterService.Setup(x => x.Decrypt(It.IsAny<Guid>(), It.IsAny<Stream>()))
                .ReturnsAsync(contentBytes);

            var sut = new EncryptedJsonInputFormatter(moqCrypterService.Object);

            var result = await sut.ReadRequestBodyAsync(inputFormatterContext);

            result
                .Should().BeEquivalentTo(InputFormatterResult.Success(request));

            moqHeaders.Verify(x => x.TryGetValue(Constants.RSA_IDENTIFIER_HEADER, out certId), Times.Once);

            moqCrypterService.Verify(x => x.Decrypt(It.IsAny<Guid>(), It.IsAny<Stream>()), Times.Once);
        }

        private static (Mock<IHeaderDictionary> moqHeaders, Mock<HttpContext> moqHttpContext) GetHttpContext(byte[] contentBytes, string contentType = Constants.ENCRYPTED_CONTENT_TYPE)
        {
            var request = new Mock<HttpRequest>();
            var headers = new Mock<IHeaderDictionary>();
            request.SetupGet(r => r.Headers).Returns(headers.Object);
            request.SetupGet(f => f.Body).Returns(new MemoryStream(contentBytes));
            request.SetupGet(f => f.ContentType).Returns(contentType);

            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            return (headers, httpContext);
        }

        private static ModelMetadata GetModelMetadata(Type modelType)
        {
            var provider = new EmptyModelMetadataProvider();
            var detailsProvider = new DefaultCompositeMetadataDetailsProvider(
                Enumerable.Empty<IMetadataDetailsProvider>());

            var key = ModelMetadataIdentity.ForType(modelType);
            var cache = new DefaultMetadataDetails(key, ModelAttributes.GetAttributesForType(modelType));

            return new DefaultModelMetadata(provider, detailsProvider, cache);
        }
    }
}
