using FluentAssertions;
using GuimoSoft.Cryptography.RSA.Exceptions;
using GuimoSoft.Cryptography.RSA.Packets;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GuimoSoft.Cryptography.Tests
{
    public class PasswordEncryptedPackageTests
    {
        private const string TEST_PASSWORD = "çd25)}%jnsgdf";
        private static readonly ReadOnlyMemory<byte> TEST_BYTES_CONTENT = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("Mensagem de teste 123"));

        [Fact]
        public void Dado_UmPacoteNulo_Se_Construir_Entao_EstouraArgumentnullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PasswordEncryptedPackage(default));
        }

        [Fact]
        public void Dado_UmPacoteComOConteudoModificado_Se_CriarPacote_Entao_EstouraCorruptedPackageException()
        {

            var package = new PasswordEncryptedPackage(TEST_BYTES_CONTENT.ToArray());
            var bytes = package.Encrypt(TEST_PASSWORD);
            bytes[8] = (byte)(bytes[8] ^ 112);
            bytes[9] = (byte)(bytes[9] ^ 112);
            bytes[10] = (byte)(bytes[10] ^ 112);

            var newPackage = new PasswordEncryptedPackage(bytes);

            Assert.Throws<CorruptedPackageException>(() => newPackage.Decrypt(TEST_PASSWORD));
        }

        [Fact]
        public async Task Dado_UmPacoteComOConteudoModificado_Se_CriarPacoteAsync_Entao_EstouraCorruptedPackageException()
        {
            var package = new PasswordEncryptedPackage(TEST_BYTES_CONTENT.ToArray());
            var bytes = await package.EncryptAsync(TEST_PASSWORD);
            bytes[8] = (byte)(bytes[8] ^ 112);
            bytes[9] = (byte)(bytes[9] ^ 112);
            bytes[10] = (byte)(bytes[10] ^ 112);

            var newPackage = new PasswordEncryptedPackage(bytes);

            await Assert.ThrowsAsync<CorruptedPackageException>(async () => await newPackage.DecryptAsync(TEST_PASSWORD));
        }

        [Fact]
        public void Dado_UmPacoteComOInicioRemovido_Se_CriarPacote_Entao_EstouraInsufficientContentInStreamException()
        {
            var package = new PasswordEncryptedPackage(TEST_BYTES_CONTENT.ToArray());
            var bytes = package.Encrypt(TEST_PASSWORD).Skip(5).ToArray();

            Assert.Throws<InsufficientContentInStreamException>(() => new RsaParametersPackage(bytes));
        }

        [Fact]
        public async Task Dado_UmPacoteComOInicioRemovido_Se_CriarPacoteAsync_Entao_EstouraInsufficientContentInStreamException()
        {
            var package = new PasswordEncryptedPackage(TEST_BYTES_CONTENT.ToArray());
            var bytes = (await package.EncryptAsync(TEST_PASSWORD)).Skip(5).ToArray();

            Assert.Throws<InsufficientContentInStreamException>(() => new RsaParametersPackage(bytes));
        }

        [Fact]
        public void Dado_UmPacoteComOFinalRemovido_Se_CriarPacote_Entao_EstouraInsufficientContentInStreamException()
        {
            var package = new PasswordEncryptedPackage(TEST_BYTES_CONTENT.ToArray());
            var bytes = package.Encrypt(TEST_PASSWORD);
            bytes = bytes.Take(bytes.Length - 5).ToArray();

            Assert.Throws<InsufficientContentInStreamException>(() => new RsaParametersPackage(bytes));
        }

        [Fact]
        public async Task Dado_UmPacoteComOFinalRemovido_Se_CriarPacoteAsync_Entao_EstouraInsufficientContentInStreamException()
        {
            var package = new PasswordEncryptedPackage(TEST_BYTES_CONTENT.ToArray());
            var bytes = await package.EncryptAsync(TEST_PASSWORD);
            bytes = bytes.Take(bytes.Length - 5).ToArray();

            Assert.Throws<InsufficientContentInStreamException>(() => new RsaParametersPackage(bytes));
        }

        [Fact]
        public void Dado_UmPacoteIntegro_Se_CriarPacote_Entao_DeserializaSemErros()
        {
            var package = new PasswordEncryptedPackage(TEST_BYTES_CONTENT.ToArray());

            var encryptedContent = package.Encrypt(TEST_PASSWORD);

            encryptedContent
                .Should().NotBeEquivalentTo(TEST_BYTES_CONTENT.ToArray());

            var newPackage = new PasswordEncryptedPackage(encryptedContent);

            var decryptedContent = newPackage.Decrypt(TEST_PASSWORD);

            decryptedContent
                .Should().BeEquivalentTo(TEST_BYTES_CONTENT.ToArray());
        }

        [Fact]
        public async Task Dado_UmPacoteIntegro_Se_CriarPacoteAsync_Entao_DeserializaSemErros()
        {
            var package = new PasswordEncryptedPackage(TEST_BYTES_CONTENT.ToArray());

            var encryptedContent = await package.EncryptAsync(TEST_PASSWORD);

            encryptedContent
                .Should().NotBeEquivalentTo(TEST_BYTES_CONTENT.ToArray());

            var newPackage = new PasswordEncryptedPackage(encryptedContent);

            var decryptedContent = await newPackage.DecryptAsync(TEST_PASSWORD);

            decryptedContent
                .Should().BeEquivalentTo(TEST_BYTES_CONTENT.ToArray());
        }
    }
}
