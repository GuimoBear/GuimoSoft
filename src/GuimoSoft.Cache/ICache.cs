namespace GuimoSoft.Cache
{
    public interface ICache
    {
        CacheItemBuilder<TKey, TValue> Get<TKey, TValue>(TKey key);
    }
}
