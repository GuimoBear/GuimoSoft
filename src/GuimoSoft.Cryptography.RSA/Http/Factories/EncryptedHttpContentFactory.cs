using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GuimoSoft.Cryptography.RSA.Exceptions;
using GuimoSoft.Cryptography.RSA.Services.Interfaces;

namespace GuimoSoft.Cryptography.RSA.Http.Factories
{
    public class EncryptedHttpContentFactory
    {
        private readonly Guid _id;
        private readonly ICrypterService _crypter;

        public EncryptedHttpContentFactory(Guid id, ICrypterService crypter)
        {
            _id = id;
            _crypter = crypter ?? throw new ArgumentNullException(nameof(crypter));
        }

        public EncryptedJsonHttpContent<TRequestType> CreateRequestContent<TRequestType>(TRequestType body)
        {
            return new EncryptedJsonHttpContent<TRequestType>(body, _id, _crypter);
        }

        public async Task<TResponseType> GetResponseObject<TResponseType>(HttpResponseMessage response)
        {
            var encryptedContent = await response.Content.ReadAsStreamAsync();

            var stringResponseCertificateIdentifier = response.Headers.GetValues(Constants.RSA_IDENTIFIER_HEADER).FirstOrDefault();
            if (!Guid.TryParse(stringResponseCertificateIdentifier, out var responseCertificateIdentifier))
                throw new RsaIdentifierNotInformedException();

            var decryptedContent = await _crypter.Decrypt(responseCertificateIdentifier, encryptedContent);
            var stringContent = Encoding.UTF8.GetString(decryptedContent);
            return JsonConvert.DeserializeObject<TResponseType>(stringContent);
        }
    }
}
