using System;
using System.Collections.Generic;

namespace TypeMember.TinyCache
{
    public abstract class TinyCacheBase<TKey> : ITinyCache<TKey>
    {
        public abstract IEnumerable<KeyValuePair<TKey, object>> Items { get; }

        public abstract bool IsItemCached(TKey key);

        public abstract TItem GetItem<TItem>(TKey key);

        public virtual TItem GetOrSetItem<TItem>(TKey key, Func<TItem> itemFunc)
        {
            if (IsItemCached(key))
            {
                return GetItem<TItem>(key);
            }
            var item = itemFunc();
            SetItem(key, item);
            return item;
        }

        public abstract void SetItem<TItem>(TKey key, TItem item);

        public virtual void SetItem<TItem>(TKey key, Func<TItem> itemFunc)
        {
            var item = itemFunc();
            SetItem(key, item);
        }

        public virtual void SetItems<TItem>(IEnumerable<KeyValuePair<TKey, TItem>> items)
        {
            foreach (var kvp in items)
            {
                SetItem(kvp.Key, kvp.Value);
            }
        }

        public abstract bool RemoveItem(TKey key);

        public abstract bool RemoveItem(TKey key, out object item);
    }
}