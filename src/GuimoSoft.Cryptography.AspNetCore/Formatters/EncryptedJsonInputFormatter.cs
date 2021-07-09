using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GuimoSoft.Cryptography.RSA.Services.Interfaces;
using GuimoSoft.Cryptography.RSA.Utils;

namespace GuimoSoft.Cryptography.AspNetCore.Formatters
{
    public class EncryptedJsonInputFormatter : InputFormatter
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            Error = HandleDeserializationIgnoreError
        };

        private readonly ICrypterService service;

        public EncryptedJsonInputFormatter(ICrypterService service) : base()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(Constants.ENCRYPTED_CONTENT_TYPE));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(Constants.ENCRYPTED_BASE64_CONTENT_TYPE));
            this.service = service;
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            MemoryStream ms = default;
            try
            {
                if (context.HttpContext.Request.Headers.TryGetValue(Constants.RSA_IDENTIFIER_HEADER, out var rsaIdString) &&
                   Guid.TryParse(rsaIdString, out var id))
                {
                    var body = context.HttpContext.Request.Body;
                    if (context.HttpContext.Request.ContentType.StartsWith(Constants.ENCRYPTED_BASE64_CONTENT_TYPE))
                    {
                        ms = new MemoryStream(Convert.FromBase64String(await body.ReadAsStringAsync()));
                        ms.Position = 0;
                        body = ms;
                    }
                    var strContent = Encoding.UTF8.GetString(await service.Decrypt(id, body));
                    var model = JsonConvert.DeserializeObject(strContent, context.ModelType, jsonSerializerSettings);
                    return InputFormatterResult.Success(model);
                }
                return InputFormatterResult.Failure();
            }
            finally
            {
                if (ms != default)
                    await ms.DisposeAsync();
            }
        }

        private static void HandleDeserializationIgnoreError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
        {
            errorArgs.ErrorContext.Handled = true;
        }
    }
}
