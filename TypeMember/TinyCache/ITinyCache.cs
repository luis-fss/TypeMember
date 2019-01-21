using System;
using System.Collections.Generic;

namespace TypeMember.TinyCache
{
    public interface ITinyCache<TKey>
    {
        IEnumerable<KeyValuePair<TKey, object>> Items { get; }
        bool IsItemCached(TKey key);
        TItem GetItem<TItem>(TKey key);
        TItem GetOrSetItem<TItem>(TKey key, Func<TItem> itemFunc);
        void SetItem<TItem>(TKey key, TItem item);
        void SetItem<TItem>(TKey key, Func<TItem> itemFunc);
        void SetItems<TItem>(IEnumerable<KeyValuePair<TKey, TItem>> items);
        bool RemoveItem(TKey key);
        bool RemoveItem(TKey key, out object item);
    }
}