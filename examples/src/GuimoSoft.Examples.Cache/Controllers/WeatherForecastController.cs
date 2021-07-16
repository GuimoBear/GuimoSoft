using GuimoSoft.Cache;
using GuimoSoft.Examples.Cache.ValueObjects;
using GuimoSoft.Logger;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuimoSoft.Examples.Cache.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IApiLogger<WeatherForecastController> _logger;
        private readonly ICache _cache;

        public WeatherForecastController(IApiLogger<WeatherForecastController> logger, ICache cache)
        {
            _logger = logger;
            _cache = cache;
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
        public async Task<WeatherForecast> Post([FromBody] WeatherForecastRequest request)
        {
            return await _cache
                .Get<WeatherForecastRequest, WeatherForecast>(request)
                .OrAddAsync(async () =>
                {
                    _logger
                        .ComPropriedade("datetime", DateTime.Now)
                        .Informacao($"criando uma nova instância da previsão do tempo para a cidade {request.City} no dia {request.Date.ToString("dd/MM/yyyy")}");
                    var rng = new Random();
                    await Task.Delay(TimeSpan.FromMilliseconds(rng.Next(500, 1500)));
                    return new WeatherForecast
                    {
                        Date = request.Date,
                        TemperatureC = rng.Next(-20, 55),
                        Summary = request.City
                    };
                });

        }
    }
}
