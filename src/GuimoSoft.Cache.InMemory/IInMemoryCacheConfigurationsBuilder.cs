using System;
using System.Collections.Generic;

namespace GuimoSoft.Cache
{
    public interface IInMemoryCacheConfigurationsBuilder<TKey, TValue>
    {
        IInMemoryCacheConfigurationsBuilder<TKey, TValue> WithTTL(TimeSpan ttl);
        IInMemoryCacheConfigurationsBuilder<TKey, TValue> WithKeyEqualityComparer(IEqualityComparer<TKey> equalityComparer);
        IInMemoryCacheConfigurationsBuilder<TKey, TValue> ShareValuesBetweenKeys(IEqualityComparer<TValue> equalityComparer);
        IInMemoryCacheConfigurationsBuilder<TKey, TValue> WithCleaner(TimeSpan cleaningInterval, TimeSpan delayToNextCancellationRequestedCheck = default);
        IInMemoryCacheConfigurationsBuilder<TKey, TValue> UsingValueFactoryProxy(IValueFactoryProxy<TValue> valueFactoryProxy);
    }
}
