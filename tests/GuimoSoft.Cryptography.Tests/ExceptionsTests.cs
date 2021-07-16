using FluentAssertions;
using GuimoSoft.Cryptography.RSA.Exceptions;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace GuimoSoft.Cryptography.Tests
{
    public class ExceptionsTests
    {
        [Fact]
        public void Se_ConstruirCorruptedPackageException_Entao_NaoEstouraErro()
        {
            _ = new CorruptedPackageException("");
            SerializeTest(new CorruptedPackageException());
        }

        [Fact]
        public void Se_ConstruirInsufficientContentInStreamException_Entao_NaoEstouraErro()
        {
            _ = new InsufficientContentInStreamException("");
            SerializeTest(new InsufficientContentInStreamException());
        }

        [Fact]
        public void Se_ConstruirRsaIdentifierNotInformedException_Entao_NaoEstouraErro()
        {
            SerializeTest(new RsaIdentifierNotInformedException());
        }

        private void SerializeTest<TException>(TException ex) where TException : Exception
        {
            using var mem = new MemoryStream();
            var bf = new BinaryFormatter();
#pragma warning disable SYSLIB0011 // Type or member is obsolete
            bf.Serialize(mem, ex);
#pragma warning restore SYSLIB0011 // Type or member is obsolete

            mem.Position = 0;

#pragma warning disable SYSLIB0011 // Type or member is obsolete
            var newEx = bf.Deserialize(mem);
#pragma warning restore SYSLIB0011 // Type or member is obsolete

            newEx
                .Should().BeEquivalentTo(ex);
        }
    }
}
