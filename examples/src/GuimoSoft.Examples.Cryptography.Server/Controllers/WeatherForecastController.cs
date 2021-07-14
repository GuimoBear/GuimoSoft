using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using GuimoSoft.Cryptography.AspNetCore;
using GuimoSoft.Examples.Cryptography.Core;

namespace GuimoSoft.Examples.Cryptography.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        [Consumes(Constants.ENCRYPTED_CONTENT_TYPE, Constants.ENCRYPTED_BASE64_CONTENT_TYPE)]
        [Produces(Constants.ENCRYPTED_CONTENT_TYPE, Constants.ENCRYPTED_BASE64_CONTENT_TYPE)]
        public WeatherForecast Post([FromBody] WeatherForecastRequest request)
        {
            var rng = new Random();
            return new WeatherForecast
            {
                Date = request.Date,
                TemperatureC = rng.Next(-20, 55),
                Summary = request.City
            };
        }
    }
}
