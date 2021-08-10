using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GuimoSoft.Cryptography.AspNetCore;
using GuimoSoft.Cryptography.RSA.Exceptions;
using GuimoSoft.Cryptography.RSA.Http.Factories;
using GuimoSoft.Cryptography.RSA.Services.Interfaces;
using GuimoSoft.Cryptography.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Cryptography.Tests
{
    public class EncryptedHttpContentFactoryTests
    {
        [Fact]
        public void When_CrypterServiceIsNull_Then_ConstructThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EncryptedHttpContentFactory(default, null));
        }

        [Fact]
        public async Task EncryptedHttpContentFactoryFacts()
        {
            var certId = Guid.NewGuid();
            var expected = new FakeRequest("t", 1);

            var responseBytes = new byte[0];

            var moqCrypterService = new Mock<ICrypterService>();
            moqCrypterService
                .Setup(x => x.Encrypt(certId, It.IsAny<byte[]>()))
                .ReturnsAsync(responseBytes);

            moqCrypterService
                .Setup(x => x.Decrypt(certId, It.IsAny<Stream>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(expected)));

            var sut = new EncryptedHttpContentFactory(certId, moqCrypterService.Object);

            var content = sut.CreateRequestContent(expected);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            responseMessage.Content = content;
            responseMessage.Headers.Add(Constants.RSA_IDENTIFIER_HEADER, certId.ToString());

            var actual = await sut.GetResponseObject<FakeRequest>(responseMessage);

            actual
                .Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task When_GetResponseObjectWithoutCertIdentifierHeader_Then_ThrowRsaIdentifierNotInformedException()
        {
            var certId = Guid.NewGuid();
            var expected = new FakeRequest("t", 1);

            var responseBytes = new byte[0];

            var moqCrypterService = new Mock<ICrypterService>();
            moqCrypterService
                .Setup(x => x.Encrypt(certId, It.IsAny<byte[]>()))
                .ReturnsAsync(responseBytes);

            moqCrypterService
                .Setup(x => x.Decrypt(certId, It.IsAny<Stream>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(expected)));

            var sut = new EncryptedHttpContentFactory(certId, moqCrypterService.Object);

            var content = sut.CreateRequestContent(expected);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            responseMessage.Content = content;

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetResponseObject<FakeRequest>(responseMessage));

            responseMessage.Headers.Add(Constants.RSA_IDENTIFIER_HEADER, certId.ToString().Substring(0, 15));

            await Assert.ThrowsAsync<RsaIdentifierNotInformedException>(() => sut.GetResponseObject<FakeRequest>(responseMessage));
        }
    }
}
