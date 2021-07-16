using GuimoSoft.Cryptography.RSA.Http.Factories;
using GuimoSoft.Cryptography.RSA.Services;
using GuimoSoft.Examples.Cryptography.Core;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GuimoSoft.Examples.Cryptography.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            EnvFile.CarregarVariaveis();
            var repository = new EnvironmentVariableRsaParametersRepository("CERT_", Environment.GetEnvironmentVariable("RSA_PASSWORD"));
            Guid.TryParse(Environment.GetEnvironmentVariable("RSA_DEFAULT_CERTIFICATE"), out var certificateIdentifier);

            var crypter = new CrypterService(repository);

            var contentFactory = new EncryptedHttpContentFactory(certificateIdentifier, crypter);

            using var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

            var request = new HttpRequestMessage(HttpMethod.Post, "/weatherforecast");
            request.Content = contentFactory.CreateRequestContent(new WeatherForecastRequest { City = "teste", Date = DateTime.Today });

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var forecast = await contentFactory.GetResponseObject<WeatherForecast>(response);
                Console.WriteLine($"Previsão para {forecast.Summary}: {forecast.TemperatureC} ºC");
            }
        }
    }
}
