using System.IO;
using System.Text;
using System.Threading.Tasks;
using System;

namespace GuimoSoft.Cryptography.RSA.Utils
{
    public static class StreamExtensions
    {
        public static async ValueTask<string> ReadAsStringAsync(this Stream stream, Encoding encoding = default)
        {
            encoding ??= Encoding.UTF8;
            using var ms = new MemoryStream();
            var buffer = new byte[2048];
            var count = 0;
            while ((count = await stream.ReadAsync(buffer.AsMemory(0, 2048))) > 0)
                await ms.WriteAsync(buffer.AsMemory(0, count));
            ms.Position = 0;

            return encoding.GetString(ms.ToArray());
        }
    }
}
