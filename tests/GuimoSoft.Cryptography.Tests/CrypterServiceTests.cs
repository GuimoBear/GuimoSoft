using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GuimoSoft.Cryptography.AspNetCore;
using GuimoSoft.Cryptography.RSA.Exceptions;
using GuimoSoft.Cryptography.RSA.Packets;
using GuimoSoft.Cryptography.RSA.Repositories.Interfaces;
using GuimoSoft.Cryptography.RSA.Services;
using Xunit;

namespace GuimoSoft.Cryptography.Tests
{
    public class CrypterServiceTests
    {
        private const string longEvent = @"Lorem ipsum dolor sit amet, id nisl delicata scriptorem est, ex duo feugait mentitum, ad quo tempor luptatum. Ferri admodum intellegebat pri et, et eos nusquam eligendi moderatius, et vim nisl posse commodo. Ei pri phaedrum laboramus expetendis, ei sed error verear aperiri. Ullum suavitate imperdiet no ius, eam in epicurei mediocrem, ea per veri mutat aliquando. Nisl sumo fuisset ex vix, pri ad meis principes constituto.
Etiam latine ut usu, vidit labitur ex vim, eum alii principes forensibus ex. Has putent dissentias ne. An pertinacia suscipiantur nam. Dicunt antiopam molestiae in duo, mel meliore omnesque et. Nam choro gloriatur ea, impedit dolores menandri nam et. Meis dignissim concludaturque vis ne, vix ut omnium elaboraret, mei debet iracundia ut.";

        public Guid Identifier { get; } = Guid.NewGuid();
        public RSAParameters PublicAndPrivateRSA2048Parameters { get; set; }
        public RSAParameters PublicRSA2048Parameters { get; }

        public CrypterServiceTests()
        {
            (PublicRSA2048Parameters, PublicAndPrivateRSA2048Parameters) = CreateKeys();

            var _parameters = new RsaParametersPackage(new RsaParametersPackage(Identifier, PublicAndPrivateRSA2048Parameters).Bytes).Content;
        }

        [Fact]
        public async Task Se_Create_Entao_NaoEstouraErro()
        {
            var (_, _, _) = await CrypterService.Create();
            var (_, _, _) = await CrypterService.Create("teste@123");
        }

        [Fact]
        public async Task Dado_UmIdQueNaoExiste_Se_Encrypt_Entao_EstouraExcecao()
        {
            var _parameters = new RsaParametersPackage(new RsaParametersPackage(Identifier, PublicAndPrivateRSA2048Parameters).Bytes).Content;
            var moqRepository = new Mock<IRsaParametersRepository>();
            moqRepository.Setup(x => x.TentarObterPorId(Identifier, out _parameters))
                .Returns(false);

            var sut = new CrypterService(moqRepository.Object);

            await Assert.ThrowsAsync<RsaIdentifierNotInformedException>(async () => await sut.Encrypt(Identifier, new byte[0]));
        }

        [Fact]
        public async Task Dado_RSAParametersQueNaoPermitemEncriptar_Se_Encrypt_Entao_EstouraExcecao()
        {
            var _parameters = new RsaParametersPackage(new RsaParametersPackage(Identifier, PublicAndPrivateRSA2048Parameters).Bytes).Content;
            _parameters.Modulus = new byte[0];
            _parameters.Exponent = new byte[0];
            var moqRepository = new Mock<IRsaParametersRepository>();
            moqRepository.Setup(x => x.TentarObterPorId(Identifier, out _parameters))
                .Returns(true);

            var sut = new CrypterService(moqRepository.Object);

            await Assert.ThrowsAsync<RsaIdentifierNotInformedException>(async () => await sut.Encrypt(Identifier, new byte[0]));
        }

        [Fact]
        public async Task Dado_TodosOsParametrosValidos_Se_Encrypt_Entao_RetornaArrayCriptografado()
        {
            var _parameters = new RsaParametersPackage(new RsaParametersPackage(Identifier, PublicAndPrivateRSA2048Parameters).Bytes).Content;
            var moqRepository = new Mock<IRsaParametersRepository>();
            moqRepository.Setup(x => x.TentarObterPorId(Identifier, out _parameters))
                .Returns(true);

            var sut = new CrypterService(moqRepository.Object);

            var encryptedContent = await sut.Encrypt(Identifier, Encoding.UTF8.GetBytes(longEvent));

            var decodedEvent = await GetDecodedEvent(encryptedContent);

            decodedEvent
                .Should().Be(longEvent);
        }

        [Fact]
        public async Task Dado_UmHttpRequestComCorpoVazio_Se_Decrypt_Entao_RetornaUmArrayVazio()
        {
            var context = new DefaultHttpContext();
            var _parameters = new RsaParametersPackage(new RsaParametersPackage(Identifier, PublicAndPrivateRSA2048Parameters).Bytes).Content;

            var moqRepository = new Mock<IRsaParametersRepository>();
            moqRepository.Setup(x => x.TentarObterPorId(Identifier, out _parameters))
                .Returns(true);

            var sut = new CrypterService(moqRepository.Object);

            context.Request.Headers.Add(Constants.RSA_IDENTIFIER_HEADER, Identifier.ToString());
            context.Request.Body = new MemoryStream();

            var result = await sut.Decrypt(Identifier, context.Request.Body);

            result
                .Should().BeEmpty();
        }

        [Fact]
        public async Task Dado_RepositoryQueNaoEncontraOCertificado_Se_Decrypt_Entao_EstouraExcecao()
        {
            var context = new DefaultHttpContext();
            var _parameters = new RsaParametersPackage(new RsaParametersPackage(Identifier, PublicAndPrivateRSA2048Parameters).Bytes).Content;

            var moqRepository = new Mock<IRsaParametersRepository>();
            moqRepository.Setup(x => x.TentarObterPorId(Identifier, out _parameters))
                .Returns(false);

            var sut = new CrypterService(moqRepository.Object);

            context.Request.Headers.Add(Constants.RSA_IDENTIFIER_HEADER, Identifier.ToString());

            await Assert.ThrowsAsync<RsaIdentifierNotInformedException>(async () => await sut.Decrypt(Identifier, context.Request.Body));
        }

        [Fact]
        public async Task Dado_RSAParametersQueNaoPermitemDecriptar_Se_Decrypt_Entao_EstouraExcecao()
        {
            var context = new DefaultHttpContext();
            var _parameters = new RsaParametersPackage(new RsaParametersPackage(Identifier, PublicAndPrivateRSA2048Parameters).Bytes).Content;
            _parameters.D = new byte[0];
            var moqRepository = new Mock<IRsaParametersRepository>();
            moqRepository.Setup(x => x.TentarObterPorId(Identifier, out _parameters))
                .Returns(true);

            var sut = new CrypterService(moqRepository.Object);

            context.Request.Headers.Add(Constants.RSA_IDENTIFIER_HEADER, Identifier.ToString());

            await Assert.ThrowsAsync<RsaIdentifierNotInformedException>(async () => await sut.Decrypt(Identifier, context.Request.Body));
        }

        [Fact]
        public async Task Dado_UmHttpRequestComCorpoInvalido_Se_Decrypt_Entao_EstouraCryptographicException()
        {
            var context = new DefaultHttpContext();
            var _parameters = new RsaParametersPackage(new RsaParametersPackage(Identifier, PublicAndPrivateRSA2048Parameters).Bytes).Content;

            var moqRepository = new Mock<IRsaParametersRepository>();
            moqRepository.Setup(x => x.TentarObterPorId(Identifier, out _parameters))
                .Returns(true);

            var sut = new CrypterService(moqRepository.Object);

            context.Request.Headers.Add(Constants.RSA_IDENTIFIER_HEADER, Identifier.ToString());
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("Isso é uma string invalida"));
            await Assert.ThrowsAsync<CryptographicException>(async () => await sut.Decrypt(Identifier, context.Request.Body));
        }

        [Fact]
        public async Task Dado_UmHttpRequestComCorpoEncriptografado_Se_Decrypt_Entao_RetornaUmArrayValido()
        {
            var context = new DefaultHttpContext();
            var _parameters = new RsaParametersPackage(new RsaParametersPackage(Identifier, PublicAndPrivateRSA2048Parameters).Bytes).Content;

            var moqRepository = new Mock<IRsaParametersRepository>();
            moqRepository.Setup(x => x.TentarObterPorId(Identifier, out _parameters))
                .Returns(true);

            var sut = new CrypterService(moqRepository.Object);

            var encodedContent = GetEncodedEvent(longEvent);

            context.Request.Headers.Add(Constants.RSA_IDENTIFIER_HEADER, Identifier.ToString());
            var ms = new MemoryStream(encodedContent);
            ms.Position = 0;
            context.Request.Body = ms;
            var result = await sut.Decrypt(Identifier, context.Request.Body);

            var @event = Encoding.UTF8.GetString(result);

            @event
                .Should().Be(longEvent); 
            
            ms = new MemoryStream(encodedContent);
            ms.Position = 0;
            context.Request.Body = ms;

            result = await sut.Decrypt(Identifier, context.Request.Body);

            @event = Encoding.UTF8.GetString(result);

            @event
                .Should().Be(longEvent);
        }

        private byte[] GetEncodedEvent(string @event)
        {
            var content = Encoding.UTF8.GetBytes(@event);

            using var ms = new MemoryStream(content);
            ms.Position = 0;
            using var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(PublicRSA2048Parameters);

            using var msOutput = new MemoryStream();
            var buffer = new byte[CrypterService.MAX_ENCRYPTED_PACKAGE_LENGTH];
            var count = 0;
            while ((count = ms.Read(buffer, 0, CrypterService.MAX_ENCRYPTED_PACKAGE_LENGTH)) > 0)
            {
                var encodedPackage = rsa.Encrypt(buffer.Take(count).ToArray(), CrypterService.USE_OAEP);
                msOutput.Write(encodedPackage, 0, encodedPackage.Length);
            }
            msOutput.Position = 0;
            return msOutput.ToArray();
        }

        private async ValueTask<string> GetDecodedEvent(byte[] content)
        {
            using var inputStream = new MemoryStream(content);
            using var outputStream = new MemoryStream(); 
            using var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(PublicAndPrivateRSA2048Parameters);

            inputStream.Position = 0;
            var buffer = new byte[256];
            var count = 0;
            while ((count = await inputStream.ReadAsync(buffer)) > 0)
            {
                var bytes = buffer.Take(count).ToArray();
                await outputStream.WriteAsync(rsa.Decrypt(bytes, CrypterService.USE_OAEP));
            }
            outputStream.Position = 0;
            return Encoding.UTF8.GetString(outputStream.ToArray());
        }

        private (RSAParameters publicKey, RSAParameters publicAndPrivateKey) CreateKeys()
        {
            using var rsa = new RSACryptoServiceProvider(2048);
            return (rsa.ExportParameters(includePrivateParameters: false), rsa.ExportParameters(includePrivateParameters: true));
        }
    }
}
