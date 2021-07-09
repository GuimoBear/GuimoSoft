using System;
using System.IO;
using System.Threading.Tasks;

namespace GuimoSoft.Cryptography.RSA.Services.Interfaces
{
    public interface ICrypterService
    {
        ValueTask<byte[]> Encrypt(Guid identifier, byte[] content);
        ValueTask<byte[]> Decrypt(Guid identifier, Stream content);
    }
}
