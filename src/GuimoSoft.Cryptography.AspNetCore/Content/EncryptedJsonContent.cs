using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GuimoSoft.Cryptography.RSA.Services.Interfaces;

namespace GuimoSoft.Cryptography.AspNetCore.Content
{
    public class EncryptedJsonContent : HttpContent
    {
        private readonly Guid _id;

        private readonly Lazy<ValueTask<byte[]>> lazyEncryptedContent;

        public EncryptedJsonContent(byte[] content, Guid id, ICrypterService service) : base()
        {
            _id = id;
            Headers.TryAddWithoutValidation(Constants.RSA_IDENTIFIER_HEADER, _id.ToString());
            Headers.ContentType = MediaTypeHeaderValue.Parse(Constants.ENCRYPTED_CONTENT_TYPE);

            lazyEncryptedContent = new Lazy<ValueTask<byte[]>>(() => service.Encrypt(_id, content));
        }
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var content = (await lazyEncryptedContent.Value).AsMemory();
            await stream.WriteAsync(content);
        }

        protected override bool TryComputeLength(out long length)
        {
            var content = lazyEncryptedContent.Value
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            length = content.Length;
            return true;
        }
    }
}
