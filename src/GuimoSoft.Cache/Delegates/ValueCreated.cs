using GuimoSoft.Cache.Utils;

namespace GuimoSoft.Cache.Delegates
{
    internal delegate void ValueCreated<in TKey, TValue>(TKey key, CacheState cacheState, ref TValue value);
}
