using GuimoSoft.Cryptography.RSA.Exceptions;
using GuimoSoft.Cryptography.RSA.Packets;
using GuimoSoft.Cryptography.RSA.Repositories.Interfaces;
using GuimoSoft.Cryptography.RSA.Services.Interfaces;
using GuimoSoft.Cryptography.RSA.Utils;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GuimoSoft.Cryptography.RSA.Services
{
    public class CrypterService : ICrypterService
    {
        internal const int MAX_ENCRYPTED_PACKAGE_LENGTH = 214;
        internal const bool USE_OAEP = true;

        internal static readonly ConcurrentDictionary<Guid, RSACryptoServiceProvider> _cryptersCache
            = new ConcurrentDictionary<Guid, RSACryptoServiceProvider>();

        private readonly IRsaParametersRepository repository;

        public CrypterService(IRsaParametersRepository repository)
        {
            this.repository = repository;
        }

        public static async ValueTask<(Guid id, byte[] privateAndPublicRSAParameters, byte[] publicRSAParameters)> Create(string password = default)
        {
            using var rsa = new RSACryptoServiceProvider(2048);
            var id = Guid.NewGuid();
            var privateAndPublicParameters = new RsaParametersPackage(id, rsa.ExportParameters(includePrivateParameters: true));
            var publicParameters = new RsaParametersPackage(id, rsa.ExportParameters(includePrivateParameters: false));
            if (!string.IsNullOrEmpty(password))
            {
                var privateAndPublicProtectedParameters = await (new PasswordEncryptedPackage(privateAndPublicParameters.Bytes).EncryptAsync(password));
                var publicProtectedParameters = await (new PasswordEncryptedPackage(publicParameters.Bytes).EncryptAsync(password));
                return (id, privateAndPublicProtectedParameters, publicProtectedParameters);
            }

            return (id, privateAndPublicParameters.Bytes, publicParameters.Bytes);
        }

        public async ValueTask<byte[]> Encrypt(Guid identifier, byte[] content)
        {
            if (TentarObterChaveRsaPorId(identifier, parameters => parameters.CanEncrypt(), out var rsa))
            {
                using var reader = new MemoryStream(content);
                reader.Position = 0;
                using var writer = new MemoryStream();
                var buffer = new Memory<byte>(new byte[MAX_ENCRYPTED_PACKAGE_LENGTH]);
                var count = 0;
                while ((count = await reader.ReadAsync(buffer)) > 0)
                {
                    var encryptedPackage = rsa.Encrypt(buffer.Span.Slice(0, count).ToArray(), USE_OAEP).AsMemory();
                    await writer.WriteAsync(encryptedPackage);
                }
                writer.Position = 0;
                return writer.ToArray();
            }
            throw new RsaIdentifierNotInformedException();
        }

        public async ValueTask<byte[]> Decrypt(Guid identifier, Stream content)
        {
            if (TentarObterChaveRsaPorId(identifier, parameters => parameters.CanDecrypt(), out var rsa))
                return await DecryptStream(content, rsa);
            throw new RsaIdentifierNotInformedException();
        }

        private bool TentarObterChaveRsaPorId(Guid identifier, Func<RSAParameters, bool> parametersValidator, out RSACryptoServiceProvider rsa)
        {
            if (_cryptersCache.TryGetValue(identifier, out rsa))
                return true;
            if (repository.TentarObterPorId(identifier, out var parameters) &&
                parametersValidator(parameters))
            {
                rsa = new RSACryptoServiceProvider(2048);
                rsa.ImportParameters(parameters);
                _cryptersCache.TryAdd(identifier, rsa);
                return true;
            }
            return false;
        }

        private static async Task<byte[]> DecryptStream(Stream body, RSACryptoServiceProvider rsa)
        {
            using var ms = new MemoryStream();
            var buffer = new byte[256];
            var count = 0;
            while ((count = await body.ReadAsync(buffer)) > 0)
            {
                var bytes = buffer.Take(count).ToArray();
                await ms.WriteAsync(rsa.Decrypt(bytes, USE_OAEP));
            }
            ms.Position = 0;
            return ms.ToArray();
        }
    }
}
