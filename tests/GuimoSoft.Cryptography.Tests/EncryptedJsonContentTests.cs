using FluentAssertions;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GuimoSoft.Cryptography.AspNetCore;
using GuimoSoft.Cryptography.AspNetCore.Content;
using GuimoSoft.Cryptography.RSA.Services.Interfaces;
using Xunit;

namespace GuimoSoft.Cryptography.Tests
{
    public class EncryptedJsonContentTests
    {
        private const string longMessage = @"Lorem ipsum dolor sit amet, id nisl delicata scriptorem est, ex duo feugait mentitum, ad quo tempor luptatum. Ferri admodum intellegebat pri et, et eos nusquam eligendi moderatius, et vim nisl posse commodo. Ei pri phaedrum laboramus expetendis, ei sed error verear aperiri. Ullum suavitate imperdiet no ius, eam in epicurei mediocrem, ea per veri mutat aliquando. Nisl sumo fuisset ex vix, pri ad meis principes constituto.
Etiam latine ut usu, vidit labitur ex vim, eum alii principes forensibus ex. Has putent dissentias ne. An pertinacia suscipiantur nam. Dicunt antiopam molestiae in duo, mel meliore omnesque et. Nam choro gloriatur ea, impedit dolores menandri nam et. Meis dignissim concludaturque vis ne, vix ut omnium elaboraret, mei debet iracundia ut.";

        private const string fakeEncryptedMessage = "fake message";

        [Fact]
        public async Task EncryptedJsonContentFacts()
        {
            var identifier = Guid.NewGuid();
            var bytes = Encoding.UTF8.GetBytes(longMessage);
            var expectedBytes = Encoding.UTF8.GetBytes(fakeEncryptedMessage);

            var moqCrypterService = new Mock<ICrypterService>();
            moqCrypterService.Setup(x => x.Encrypt(identifier, bytes))
                .ReturnsAsync(expectedBytes);

            var sut = new EncryptedJsonContent(bytes, identifier, moqCrypterService.Object);

            sut.Headers.ContentType.ToString()
                .Should().Be(Constants.ENCRYPTED_CONTENT_TYPE);

            sut.Headers.TryGetValues(Constants.RSA_IDENTIFIER_HEADER, out var identifiers)
                .Should().BeTrue();

            identifiers.ToList()
                .Should().HaveCount(1);

            identifiers.First()
                .Should().Be(identifier.ToString());

            using var outputStream = new MemoryStream();

            await sut.CopyToAsync(outputStream);

            outputStream.Length
                .Should().Be(expectedBytes.Length);

            outputStream.Position
                .Should().Be(expectedBytes.Length);

            sut.Headers.ContentLength
                .Should().Be(expectedBytes.Length);

            outputStream.Position = 0;

            var encodedString = Encoding.UTF8.GetString(outputStream.ToArray());

            encodedString
                .Should().Be(fakeEncryptedMessage);
        }
    }
}
