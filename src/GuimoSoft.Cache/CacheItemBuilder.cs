using System.Threading.Tasks;
using GuimoSoft.Cache.Delegates;

namespace GuimoSoft.Cache
{

    public class CacheItemBuilder<TKey, TValue>
    {
        private readonly TKey _key;
        private readonly TryGetCachedItem<TKey, TValue> _tryGetCachedValueFunc;
        private readonly ValueCreated<TKey, TValue> _onValueCreated;
        private readonly IValueFactoryProxy<TValue> _valueFactoryProxy;
        private ValueFactory<TValue> _valueFactory = default;

        internal CacheItemBuilder(
            TKey key, 
            TryGetCachedItem<TKey, TValue> tryGetCachedValueFunc, 
            ValueCreated<TKey, TValue> onValueCreated, 
            IValueFactoryProxy<TValue> valueFactoryProxy)
        {
            _key = key;
            _tryGetCachedValueFunc = tryGetCachedValueFunc;
            _onValueCreated = onValueCreated;
            _valueFactoryProxy = valueFactoryProxy;
        }

        public TValue OrAdd(ValueFactory<TValue> factory)
        {
            _valueFactory = factory;
            return this;
        }

        public async Task<TValue> OrAddAsync(AsyncValueFactory<TValue> asyncFactory)
        {
            if (_tryGetCachedValueFunc(_key, out var cacheState, out var value))
                return value;
            if (asyncFactory is not null)
            {
                value = await _valueFactoryProxy.ProduceAsync(asyncFactory);
                _onValueCreated(_key, cacheState, ref value);
                return value;
            }
            return default;
        }

        private TValue Build()
        {
            if (_tryGetCachedValueFunc(_key, out var cacheState, out var value))
                return value;
            if (_valueFactory is not null)
            {
                value = _valueFactoryProxy.Produce(_valueFactory);
                _onValueCreated(_key, cacheState, ref value);
                return value;
            }
            return default;
        }

        public static implicit operator TValue(CacheItemBuilder<TKey, TValue> cacheBuilder)
        {
            if (cacheBuilder is not null)
                return cacheBuilder.Build();
            return default;
        }
    }
}
