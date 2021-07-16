using FluentAssertions;
using GuimoSoft.Cryptography.AspNetCore;
using GuimoSoft.Cryptography.AspNetCore.Formatters;
using GuimoSoft.Cryptography.RSA.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Linq;
using Xunit;

namespace GuimoSoft.Cryptography.Tests
{
    public class DependencyInjectionExtensionsTests
    {
        [Fact]
        public void AddRsaInputContentTypeFacts()
        {
            var options = new MvcOptions();

            options.AddRsaInputContentType(Mock.Of<IRsaParametersRepository>());

            options.InputFormatters
                .Should().HaveCount(1);

            var inputFormatter = options.InputFormatters.FirstOrDefault();

            inputFormatter
                .Should().NotBeNull();

            inputFormatter
                .Should().BeOfType(typeof(EncryptedJsonInputFormatter));
        }

        [Fact]
        public void AddRsaOutputContentTypeFacts()
        {
            var options = new MvcOptions();

            options.AddRsaOutputContentType(Mock.Of<IRsaParametersRepository>(), default);

            options.OutputFormatters
                .Should().HaveCount(1);

            var outputFormatter = options.OutputFormatters.FirstOrDefault();

            outputFormatter
                .Should().NotBeNull();

            outputFormatter
                .Should().BeOfType(typeof(EncryptedJsonOutputFormatter));
        }
    }
}
