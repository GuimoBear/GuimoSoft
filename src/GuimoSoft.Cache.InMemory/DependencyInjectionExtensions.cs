using GuimoSoft.Cache.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace GuimoSoft.Cache
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddInMemoryCache<TKey, TValue>(this IServiceCollection services, Action<IInMemoryCacheConfigurationsBuilder<TKey, TValue>> configurer)
        {
            services.TryAddSingleton<ITypedCache<TKey, TValue>>(new TypedCache<TKey, TValue>(configurer));

            services.TryAddTransient<ICache, Cache>();

            return services;
        }
    }
}
