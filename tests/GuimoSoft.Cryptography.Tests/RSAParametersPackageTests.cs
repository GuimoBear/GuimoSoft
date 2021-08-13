using FluentAssertions;
using System;
using System.Linq;
using GuimoSoft.Cryptography.RSA.Exceptions;
using GuimoSoft.Cryptography.RSA.Packets;
using GuimoSoft.Cryptography.Tests.Fixtures;
using Xunit;
using System.Threading.Tasks;
using System.IO;

namespace GuimoSoft.Cryptography.Tests
{
    [Collection(RSAFixturesCollection.FIXTURE_COLLECTION_NAME)]
    public class RSAParametersPackageTests
    {
        private readonly RSAFixture fixture;

        public RSAParametersPackageTests(RSAFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void Dado_UmPacoteNulo_Se_Construir_Entao_EstouraArgumentnullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RsaParametersPackage(default));
        }

        [Fact]
        public void Dado_UmPacoteComUmArrayVazio_Quando_ChamarConstrutor_Entao_LancaArgumentNullException()
        {
            Assert.Throws<InsufficientContentInStreamException>(() => new RsaParametersPackage(Array.Empty<byte>()));
        }

        [Fact]
        public void Dado_UmPacoteComApenasIdentificador_Quando_CriarPacote_Entao_LancaCorruptedPackageException()
        {
            var package = new RsaParametersPackage(fixture.Identifier, fixture.PublicRSA2048Parameters);

            Assert.Throws<InsufficientContentInStreamException>(() => new RsaParametersPackage(package.Bytes.Take(16).ToArray()));
        }

        [Fact]
        public void Dado_UmPacoteComOIdentificadorEOTamanho_Quando_CriarPacote_Entao_LancaCorruptedPackageException()
        {
            var package = new RsaParametersPackage(fixture.Identifier, fixture.PublicRSA2048Parameters);

            Assert.Throws<InsufficientContentInStreamException>(() => new RsaParametersPackage(package.Bytes.Take(18).ToArray()));
        }

        [Fact]
        public void Dado_UmPacoteSemPayload_Quando_CriarPacote_Entao_LancaCorruptedPackageException()
        {
            var package = new RsaParametersPackage(fixture.Identifier, fixture.PublicRSA2048Parameters);

            Assert.Throws<InsufficientContentInStreamException>(() => new RsaParametersPackage(package.Bytes.Take(19).ToArray()));
        }

        [Fact]
        public void Dado_UmPacoteSemOCRC_Quando_CriarPacote_Entao_LancaCorruptedPackageException()
        {
            var package = new RsaParametersPackage(fixture.Identifier, fixture.PublicRSA2048Parameters);

            Assert.Throws<InsufficientContentInStreamException>(() => new RsaParametersPackage(package.Bytes.Take(package.Bytes.Length - 2).ToArray()));
        }

        [Fact]
        public void Dado_UmPacoteComOConteudoModificado_Se_CriarPacote_Entao_EstouraCorruptedPackageException()
        {
            var package = new RsaParametersPackage(fixture.Identifier, fixture.PublicRSA2048Parameters);
            package.Bytes[8] = (byte)(package.Bytes[8] ^ 112);
            package.Bytes[9] = (byte)(package.Bytes[9] ^ 112);
            package.Bytes[10] = (byte)(package.Bytes[10] ^ 112);

            Assert.Throws<CorruptedPackageException>(() => new RsaParametersPackage(package.Bytes));
        }

        [Fact]
        public void Dado_UmPacoteComOInicioRemovido_Se_CriarPacote_Entao_EstouraInsufficientContentInStreamException()
        {
            var package = new RsaParametersPackage(fixture.Identifier, fixture.PublicRSA2048Parameters);
            var bytes = package.Bytes.Skip(40).ToArray();

            Assert.Throws<InsufficientContentInStreamException>(() => new RsaParametersPackage(bytes));
        }

        [Fact]
        public void Dado_UmPacoteComOFinalRemovido_Se_CriarPacote_Entao_EstouraInsufficientContentInStreamException()
        {
            var package = new RsaParametersPackage(fixture.Identifier, fixture.PublicRSA2048Parameters);
            var bytes = package.Bytes.Take(package.Bytes.Length - 40).ToArray();

            Assert.Throws<InsufficientContentInStreamException>(() => new RsaParametersPackage(bytes));
        }

        [Fact]
        public void Dado_UmPacoteIntegro_Se_CriarPacote_Entao_DeserializaSemErros()
        {
            var package = new RsaParametersPackage(fixture.Identifier, fixture.PublicRSA2048Parameters);

            package.Content.Exponent
                .Should().BeEquivalentTo(fixture.PublicRSA2048Parameters.Exponent);
            package.Content.Modulus
                .Should().BeEquivalentTo(fixture.PublicRSA2048Parameters.Modulus);

            var newPackage = new RsaParametersPackage(package.Bytes);

            newPackage.Identifier
                .Should().Be(fixture.Identifier);

            newPackage.Content.Exponent
                .Should().BeEquivalentTo(fixture.PublicRSA2048Parameters.Exponent);
            newPackage.Content.Modulus
                .Should().BeEquivalentTo(fixture.PublicRSA2048Parameters.Modulus);
        }

        [Fact]
        public void Dado_UmPacoteComChavePublicaEPrivadaEComOConteudoModificado_Se_CriarPacote_Entao_EstouraCorruptedPackageException()
        {
            var package = new RsaParametersPackage(fixture.Identifier, fixture.PublicAndPrivateRSA2048Parameters);
            package.Bytes[8] = (byte)(package.Bytes[8] ^ 112);
            package.Bytes[9] = (byte)(package.Bytes[9] ^ 112);
            package.Bytes[10] = (byte)(package.Bytes[10] ^ 112);

            Assert.Throws<CorruptedPackageException>(() => new RsaParametersPackage(package.Bytes));
        }

        [Fact]
        public void Dado_UmPacoteComChavePublicaEPrivadaEComOInicioRemovido_Se_CriarPacote_Entao_EstouraInsufficientContentInStreamException()
        {
            var package = new RsaParametersPackage(fixture.Identifier, fixture.PublicAndPrivateRSA2048Parameters);
            var bytes = package.Bytes.Skip(50).ToArray();

            Assert.Throws<InsufficientContentInStreamException>(() => new RsaParametersPackage(bytes));
        }

        [Fact]
        public void Dado_UmPacoteComChavePublicaEPrivadaEComOFinalRemovido_Se_CriarPacote_Entao_EstouraInsufficientContentInStreamException()
        {
            var package = new RsaParametersPackage(fixture.Identifier, fixture.PublicAndPrivateRSA2048Parameters);
            var bytes = package.Bytes.Take(package.Bytes.Length - 40).ToArray();

            Assert.Throws<InsufficientContentInStreamException>(() => new RsaParametersPackage(bytes));
        }

        [Fact]
        public void Dado_UmPacoteComChavePublicaEPrivadaIntegro_Se_CriarPacote_Entao_DeserializaSemErros()
        {
            var package = new RsaParametersPackage(fixture.Identifier, fixture.PublicAndPrivateRSA2048Parameters);

            package.Content.D
                .Should().BeEquivalentTo(fixture.PublicAndPrivateRSA2048Parameters.D);
            package.Content.DP
                .Should().BeEquivalentTo(fixture.PublicAndPrivateRSA2048Parameters.DP);
            package.Content.DQ
                .Should().BeEquivalentTo(fixture.PublicAndPrivateRSA2048Parameters.DQ);
            package.Content.Exponent
                .Should().BeEquivalentTo(fixture.PublicAndPrivateRSA2048Parameters.Exponent);
            package.Content.InverseQ
                .Should().BeEquivalentTo(fixture.PublicAndPrivateRSA2048Parameters.InverseQ);
            package.Content.Modulus
                .Should().BeEquivalentTo(fixture.PublicAndPrivateRSA2048Parameters.Modulus);
            package.Content.P
                .Should().BeEquivalentTo(fixture.PublicAndPrivateRSA2048Parameters.P);
            package.Content.Q
                .Should().BeEquivalentTo(fixture.PublicAndPrivateRSA2048Parameters.Q);

            var newPackage = new RsaParametersPackage(package.Bytes);

            newPackage.Identifier
                .Should().Be(fixture.Identifier);

            newPackage.Content.D
                .Should().BeEquivalentTo(fixture.PublicAndPrivateRSA2048Parameters.D);
            newPackage.Content.DP
                .Should().BeEquivalentTo(fixture.PublicAndPrivateRSA2048Parameters.DP);
            newPackage.Content.DQ
                .Should().BeEquivalentTo(fixture.PublicAndPrivateRSA2048Parameters.DQ);
            newPackage.Content.Exponent
                .Should().BeEquivalentTo(fixture.PublicAndPrivateRSA2048Parameters.Exponent);
            newPackage.Content.InverseQ
                .Should().BeEquivalentTo(fixture.PublicAndPrivateRSA2048Parameters.InverseQ);
            newPackage.Content.Modulus
                .Should().BeEquivalentTo(fixture.PublicAndPrivateRSA2048Parameters.Modulus);
            newPackage.Content.P
                .Should().BeEquivalentTo(fixture.PublicAndPrivateRSA2048Parameters.P);
            newPackage.Content.Q
                .Should().BeEquivalentTo(fixture.PublicAndPrivateRSA2048Parameters.Q);
        }

        [Fact]
        public async Task Dado_UmPacoteComChavePublicaEPrivadaIntegro_Se_WriteIntoStreamAsync_Entao_ExcreveConteudoSemErro()
        {
            using var ms = new MemoryStream();

            var sut = new RsaParametersPackage(fixture.Identifier, fixture.PublicAndPrivateRSA2048Parameters);

            await sut.WriteIntoStreamAsync(ms);

            ms.Position = 0;

            ms.ToArray()
                .Should().BeEquivalentTo(sut.Bytes);
        }
    }
}
