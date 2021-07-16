using GuimoSoft.Cache.Exceptions;
using System;

namespace GuimoSoft.Cache
{
    internal class Cache : ICache
    {
        private readonly IServiceProvider _serviceProvider;

        public Cache(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public CacheItemBuilder<TKey, TValue> Get<TKey, TValue>(TKey key)
        {
            var typedCache = _serviceProvider.GetService(typeof(ITypedCache<TKey, TValue>)) as ITypedCache<TKey, TValue>;
            if (typedCache is null)
                throw new CacheNotConfiguredException(typeof(TKey), typeof(TValue));
            return typedCache.Get(key);
        }
    }
}
