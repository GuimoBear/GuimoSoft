using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using GuimoSoft.Cryptography.RSA.Services.Interfaces;

namespace GuimoSoft.Cryptography.AspNetCore.Formatters
{
    public class EncryptedJsonOutputFormatter : OutputFormatter
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        private readonly ICrypterService _service;
        private readonly Guid _identifier;

        public EncryptedJsonOutputFormatter(ICrypterService service, Guid identifier) : base()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(Constants.ENCRYPTED_CONTENT_TYPE));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(Constants.ENCRYPTED_BASE64_CONTENT_TYPE));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _identifier = identifier;
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            context.ContentType = context.HttpContext.Request.ContentType;
            var jsonContent = JsonConvert.SerializeObject(context.Object, jsonSerializerSettings);
            var utf8Bytes = Encoding.UTF8.GetBytes(jsonContent);
            var encryptedContent = await _service.Encrypt(_identifier, utf8Bytes);
            if (context.ContentType.ToString().StartsWith(Constants.ENCRYPTED_BASE64_CONTENT_TYPE))
                encryptedContent = Encoding.UTF8.GetBytes(Convert.ToBase64String(encryptedContent));
            context.HttpContext.Response.Headers.Add(Constants.RSA_IDENTIFIER_HEADER, _identifier.ToString());

            context.HttpContext.Response.ContentLength = encryptedContent.Length;
            await context.HttpContext.Response.Body.WriteAsync(encryptedContent);
        }
    }
}
