using System;
using System.IO;
using System.Text;
using System.Text.Json;
using GuimoSoft.Cryptography.RSA.Repositories.Interfaces;
using GuimoSoft.Cryptography.RSA.Services;
using GuimoSoft.Cryptography.RSA.Services.Interfaces;
using GuimoSoft.Examples.Bus.Kafka.Infra.Data.Repositories;
using GuimoSoft.Serialization.Interfaces;

namespace GuimoSoft.Examples.Bus.Kafka.Utils.Serializers
{
    public class EncryptedJsonSerializer : IDefaultSerializer
    {
        public static readonly IDefaultSerializer Instance
            = new EncryptedJsonSerializer();

        private readonly IRsaParametersRepository _repository;
        private readonly Guid _defaultCertificateId;
        private readonly ICrypterService _crypter;

        private EncryptedJsonSerializer() 
        {
            _repository = new EnvironmentVariableRsaParametersRepository(password: Environment.GetEnvironmentVariable("RSA_PASSWORD"));
            Guid.TryParse(Environment.GetEnvironmentVariable("RSA_DEFAULT_CERTIFICATE"), out _defaultCertificateId);
            _crypter = new CrypterService(_repository);
        }

        public object Deserialize(Type messageType, byte[] content)
        {
            using var ms = new MemoryStream(content);
            ms.Position = 0;
            var decryptedContent = _crypter.Decrypt(_defaultCertificateId, ms).GetAwaiter().GetResult();
            return JsonSerializer.Deserialize(Encoding.UTF8.GetString(decryptedContent), messageType);
        }

        public byte[] Serialize(object message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var encryptedContent = _crypter.Encrypt(_defaultCertificateId, messageBytes).GetAwaiter().GetResult();
            return encryptedContent;
        }
    }
}
