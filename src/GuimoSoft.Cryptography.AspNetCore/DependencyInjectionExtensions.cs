using Microsoft.AspNetCore.Mvc;
using System;
using GuimoSoft.Cryptography.AspNetCore.Formatters;
using GuimoSoft.Cryptography.RSA.Repositories.Interfaces;
using GuimoSoft.Cryptography.RSA.Services;

namespace GuimoSoft.Cryptography.AspNetCore
{
    public static class DependencyInjectionExtensions
    {
        public static MvcOptions AddRsaInputContentType(this MvcOptions options, IRsaParametersRepository repository)
        {
            var service = new CrypterService(repository);
            options.InputFormatters.Add(new EncryptedJsonInputFormatter(service));
            return options;
        }

        public static MvcOptions AddRsaOutputContentType(this MvcOptions options, IRsaParametersRepository repository, Guid defaultIdentifier)
        {
            var service = new CrypterService(repository);
            options.OutputFormatters.Add(new EncryptedJsonOutputFormatter(service, defaultIdentifier));
            return options;
        }
    }
}
