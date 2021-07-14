using System;

namespace GuimoSoft.Examples.Cache
{
    public class WeatherForecast : IEquatable<WeatherForecast>
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }

        public bool Equals(WeatherForecast other)
        {
            if (other is null)
                return false;

            return Date == other.Date &&
                   Summary == other.Summary;
        }
    }
}
