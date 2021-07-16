using GuimoSoft.Examples.Cache.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GuimoSoft.Examples.Cache.Utils
{
    public class WeatherForecastRequestEqualityComparer : IEqualityComparer<WeatherForecastRequest>
    {
        public static readonly WeatherForecastRequestEqualityComparer Instance
            = new WeatherForecastRequestEqualityComparer();

        private WeatherForecastRequestEqualityComparer() { }

        public bool Equals(WeatherForecastRequest x, WeatherForecastRequest y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;
            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] WeatherForecastRequest obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.City);
            hashCode.Add(obj.Date);
            return hashCode.ToHashCode();
        }
    }
}
