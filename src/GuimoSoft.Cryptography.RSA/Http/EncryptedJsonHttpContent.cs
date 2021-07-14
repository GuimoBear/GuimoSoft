using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using GuimoSoft.Cryptography.RSA.Services.Interfaces;

namespace GuimoSoft.Cryptography.RSA.Http
{
    public abstract class EncryptedJsonHttpContentBase : HttpContent
    {
        protected static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };
    }

    public class EncryptedJsonHttpContent<TType> : EncryptedJsonHttpContentBase
    {
        private readonly ICrypterService _crypter;

        private readonly Lazy<Task<byte[]>> lazyEncryptedContent;

        public EncryptedJsonHttpContent(TType content, Guid id, ICrypterService crypter) : base()
        {
            _crypter = crypter;
            Headers.TryAddWithoutValidation(Constants.RSA_IDENTIFIER_HEADER, id.ToString());
            Headers.ContentType = MediaTypeHeaderValue.Parse(Constants.ENCRYPTED_CONTENT_TYPE);

            lazyEncryptedContent = new Lazy<Task<byte[]>>(async () =>
            {
                var jsonContent = JsonConvert.SerializeObject(content, jsonSerializerSettings);
                var utf8Bytes = Encoding.UTF8.GetBytes(jsonContent);
                var encryptedContent = await _crypter.Encrypt(id, utf8Bytes);
                return encryptedContent;
            });
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
