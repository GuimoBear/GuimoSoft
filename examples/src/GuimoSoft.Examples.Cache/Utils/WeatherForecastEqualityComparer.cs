using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GuimoSoft.Examples.Cache.Utils
{
    public sealed class WeatherForecastEqualityComparer : IEqualityComparer<WeatherForecast>
    {
        public static readonly WeatherForecastEqualityComparer Instance 
            = new WeatherForecastEqualityComparer();

        private WeatherForecastEqualityComparer() { }

        public bool Equals(WeatherForecast x, WeatherForecast y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;
            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] WeatherForecast obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Date);
            hashCode.Add(obj.Summary);
            return hashCode.ToHashCode();
        }
    }
}
