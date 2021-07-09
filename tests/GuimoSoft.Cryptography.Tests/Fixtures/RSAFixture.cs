using System;
using System.Security.Cryptography;
using Xunit;

namespace GuimoSoft.Cryptography.Tests.Fixtures
{
    public class RSAFixture
    {
        public Guid Identifier { get; } = Guid.Parse("725271c1-d71e-43db-9a25-6cce06eda961");
        public RSAParameters PublicAndPrivateRSA2048Parameters { get; }
        public RSAParameters PublicRSA2048Parameters { get; }

        public RSAFixture()
        {
            using var rsa = new RSACryptoServiceProvider(2048);
            PublicAndPrivateRSA2048Parameters = rsa.ExportParameters(includePrivateParameters: true);
            PublicRSA2048Parameters = rsa.ExportParameters(includePrivateParameters: false);
        }
    }

    [CollectionDefinition(FIXTURE_COLLECTION_NAME)]
    public class RSAFixturesCollection : ICollectionFixture<RSAFixture>
    {
        public const string FIXTURE_COLLECTION_NAME = "RSA Parameters";
    }
}
