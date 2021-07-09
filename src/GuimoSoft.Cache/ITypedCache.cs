using System;

namespace GuimoSoft.Cache
{
    internal interface ITypedCache<TKey, TValue> : IDisposable
    {
        CacheItemBuilder<TKey, TValue> Get(TKey key);
    }
}
