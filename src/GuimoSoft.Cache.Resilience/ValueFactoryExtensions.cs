﻿using Polly;
using GuimoSoft.Cache.Resilience;

namespace GuimoSoft.Cache
{
    public static class ValueFactoryExtensions
    {
        public static IValueFactoryProxy<TValue> Resilient<TValue>(this ValueFactory _, IAsyncPolicy<TValue> asyncPolicy)
            => new ResilientValueFactoryProxy<TValue>(asyncPolicy);
    }
}
