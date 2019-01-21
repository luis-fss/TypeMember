using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TypeMember.TinyCache.Exceptions;

namespace TypeMember.TinyCache
{
    public class TinyCache<TKey> : TinyCacheBase<TKey>
    {
        private readonly ConcurrentDictionary<TKey, object> _cache;

        public TinyCache()
        {
            _cache = new ConcurrentDictionary<TKey, object>();
        }

        public override IEnumerable<KeyValuePair<TKey, object>> Items
        {
            get
            {
                return _cache.ToList();
            }
        }

        public override bool IsItemCached(TKey key)
        {
            return _cache.ContainsKey(key);
        }

        public override TItem GetItem<TItem>(TKey key)
        {
            if (IsItemCached(key))
            {
                var item = _cache[key];
                if (item is TItem)
                {
                    return (TItem)item;
                }
                throw new ItemTypeIncorrectException(key, typeof(TItem), item.GetType());
            }
            throw new ItemNotInCacheException(key);
        }

        public override void SetItem<TItem>(TKey key, TItem item)
        {
            if (!IsItemCached(key))
            {
                _cache.TryAdd(key, item);
            }
            else
            {
                _cache[key] = item;
            }
        }

        public override bool RemoveItem(TKey key)
        {
            object obj;
            return RemoveItem(key, out obj);
        }

        public override bool RemoveItem(TKey key, out object item)
        {
            if (IsItemCached(key))
            {
                return _cache.TryRemove(key, out item);
            }

            item = null;
            return false;
        }
    }
}