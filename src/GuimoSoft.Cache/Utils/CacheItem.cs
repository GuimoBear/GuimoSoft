using System;

namespace GuimoSoft.Cache.Utils
{

    internal sealed class CacheItem<TValue>
    {
        public readonly TValue Value;
        public readonly DateTime TTL;

        public CacheItem(TValue value, DateTime ttl)
        {
            Value = value;
            TTL = ttl;
        }

        public static implicit operator TValue(CacheItem<TValue> item)
        {
            if (item is not null)
                return item.Value;
            return default;
        }
    }
}
