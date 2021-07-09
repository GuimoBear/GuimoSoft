using FluentAssertions;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GuimoSoft.Cache.Exceptions;
using Xunit;

namespace GuimoSoft.Cache.Tests
{
    public class ExceptionsTests
    {
        [Fact]
        public void Se_ConstruirCacheNotConfiguredException_Entao_NaoEstouraErro()
        {
            _ = new CacheNotConfiguredException(typeof(ExceptionsTests), typeof(ExceptionsTests));
            SerializeTest(new CacheNotConfiguredException());
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
