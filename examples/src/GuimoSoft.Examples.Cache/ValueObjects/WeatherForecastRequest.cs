using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace GuimoSoft.Examples.Cache.ValueObjects
{
    public class WeatherForecastRequest : IEquatable<WeatherForecastRequest>
    {
        [JsonProperty, JsonPropertyName(nameof(City))]
        public string City { get; set; }
        [JsonProperty, JsonPropertyName(nameof(Date))]
        public DateTime Date { get; set; }

        [System.Text.Json.Serialization.JsonConstructor()]
        [Newtonsoft.Json.JsonConstructor()]
        public WeatherForecastRequest(string city, DateTime date)
        {
            City = city;
            Date = date.Date;
        }

        public bool Equals(WeatherForecastRequest other)
        {
            if (other is null)
                return false;
            return City == other.City &&
                   Date == other.Date;
        }
    }
}
