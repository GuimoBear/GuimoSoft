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
            if (context.HttpContext.Request.Headers.TryGetValue(Constants.RSA_IDENTIFIER_HEADER, out var rsaIdString) &&
                Guid.TryParse(rsaIdString, out var id))
            {
                string strContent;
                if (context.HttpContext.Request.ContentType.StartsWith(Constants.ENCRYPTED_BASE64_CONTENT_TYPE))
                {
                    using var ms = new MemoryStream(Convert.FromBase64String(await context.HttpContext.Request.Body.ReadAsStringAsync()));
                    ms.Position = 0;
                    strContent = Encoding.UTF8.GetString(await service.Decrypt(id, ms));
                }
                else
                    strContent = Encoding.UTF8.GetString(await service.Decrypt(id, context.HttpContext.Request.Body));
                var model = JsonConvert.DeserializeObject(strContent, context.ModelType, jsonSerializerSettings);
                return InputFormatterResult.Success(model);
            }
            return InputFormatterResult.Failure();
        }

        private static void HandleDeserializationIgnoreError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
        {
            errorArgs.ErrorContext.Handled = true;
        }
    }
}
