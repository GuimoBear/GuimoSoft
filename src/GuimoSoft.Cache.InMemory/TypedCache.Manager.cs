using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GuimoSoft.Cache.Utils;

namespace GuimoSoft.Cache.InMemory
{
    internal sealed partial class TypedCache<TKey, TValue>
    {
        private readonly ReaderWriterLockSlim _lock;

        private void Add(TKey key, ref TValue value, DateTime ttl)
        {
            using var locker = new DisposableLock(_lock);
            value = GetExistingOrUseCreated(value);
            _cache.TryAdd(key, new (value, ttl));
        }

        private void Update(TKey key, ref TValue value, DateTime ttl)
        {
            using var locker = new DisposableLock(_lock);
            value = GetExistingOrUseCreated(value);
            _cache[key] = new (value, ttl);
        }

        private TValue GetExistingOrUseCreated(TValue newValue)
        {
            if (_configs.ShareValuesBetweenKeys)
            {
                var existingValue = _cache.Values.FirstOrDefault(i => _configs.ValueEqualityComparer.Equals(newValue, i.Value));
                if (existingValue is not null)
                    return existingValue;
            }
            return newValue;
        }
    }
}
