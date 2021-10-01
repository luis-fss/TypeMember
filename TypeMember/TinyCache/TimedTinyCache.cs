using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TypeMember.TinyCache.Exceptions;

namespace TypeMember.TinyCache
{
    public class TimedTinyCache<TKey> : ITinyCache<TKey>, IDisposable
    {
        private readonly TinyCache<TKey> _cache;
        private readonly Timer _checkItemExpirationTimer;
        public int DefaultItemLifespanMilliseconds { get; }
        public int CheckItemExpirationIntervalMilliseconds { get; }

        public TimedTinyCache(int defaultItemLifespanMilliseconds, int checkItemExpirationIntervalMilliseconds)
        {
            if (defaultItemLifespanMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(defaultItemLifespanMilliseconds), "defaultItemLifespanMilliseconds must have a positive value");
            }
            if (checkItemExpirationIntervalMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(checkItemExpirationIntervalMilliseconds), "checkItemExpirationIntervalMilliseconds must have a positive value");
            }
            DefaultItemLifespanMilliseconds = defaultItemLifespanMilliseconds;
            CheckItemExpirationIntervalMilliseconds = checkItemExpirationIntervalMilliseconds;
            _cache = new TinyCache<TKey>();
            _checkItemExpirationTimer = new Timer(CheckItemExpiration, null, CheckItemExpirationIntervalMilliseconds, CheckItemExpirationIntervalMilliseconds);
        }

        private void CheckItemExpiration(object stateInfo)
        {
            lock (this)
            {
                var keysToRemove = _cache.Items
                    .Where(kvp => kvp.Value is not IExpirable expirable || expirable.HasExpired)
                    .Select(kvp => kvp.Key);
                foreach (var key in keysToRemove)
                {
                    RemoveItem(key);
                }
            }
        }

        public IEnumerable<KeyValuePair<TKey, object>> Items
        {
            get
            {
                CheckItemExpiration(null);
                return _cache.Items
                    .Select(kvp => 
                        kvp.Value is TimedCacheItem<object> timedCacheItem 
                            ? new KeyValuePair<TKey, object>(kvp.Key, timedCacheItem.Item)
                            : default);
            }
        }

        public bool IsItemCached(TKey key)
        {
            return _cache.IsItemCached(key) && !_cache.GetItem<IExpirable>(key).HasExpired;
        }

        #region Get itens

        public TItem GetItem<TItem>(TKey key)
        {
            if (IsItemCached(key))
            {
                return _cache.GetItem<TimedCacheItem<TItem>>(key).Item;
            }
            throw new ItemNotInCacheException(key);
        }

        public TItem GetOrSetItem<TItem>(TKey key, Func<TItem> itemFunc)
        {
            return GetOrSetItem(key, itemFunc, false);
        }

        public TItem GetOrSetItem<TItem>(TKey key, Func<TItem> itemFunc, bool autoRenew, int? lifespanMilliseconds = null)
        {
            TimedCacheItem<TItem> item = null;
            if (IsItemCached(key))
            {
                item = _cache.GetItem<TimedCacheItem<TItem>>(key);
            }
            if (item == null || (item is AutoRenewingCacheItem<TItem> && !autoRenew) || (!(item is AutoRenewingCacheItem<TItem>) && autoRenew))
            {
                item = SetItemHelper(key, item, itemFunc, autoRenew, lifespanMilliseconds);
            }
            return item.Item;
        }

        #endregion

        #region Set itens

        public void SetItem<TItem>(TKey key, TItem item)
        {
            //Cache.SetItem(key, new TimedCacheItem<TItem>(item, DefaultItemLifespanMilliseconds));
            SetItemHelper(key, null, () => item, false, null);
        }

        public void SetItem<TItem>(TKey key, TItem item, bool autoRenew, int? lifespanMilliseconds = null)
        {
            SetItemHelper(key, null, () => item, autoRenew, lifespanMilliseconds);
        }

        public void SetItem<TItem>(TKey key, Func<TItem> itemFunc)
        {
            SetItemHelper(key, null, itemFunc, false, null);
        }

        public void SetItem<TItem>(TKey key, Func<TItem> itemFunc, bool autoRenew, int? lifespanMilliseconds = null)
        {
            SetItemHelper(key, null, itemFunc, autoRenew, lifespanMilliseconds);
        }

        public void SetItems<TItem>(IEnumerable<KeyValuePair<TKey, TItem>> items)
        {
            foreach (var kvp in items)
            {
                SetItem(kvp.Key, kvp.Value);
            }
        }

        private TimedCacheItem<TItem> SetItemHelper<TItem>(TKey key, TimedCacheItem<TItem> currentItem, Func<TItem> itemFunc, bool autoRenew, int? lifespanMilliseconds)
        {
            if ((lifespanMilliseconds ?? 1) <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lifespanMilliseconds), "lifespanMilliseconds must have a positive value");
            }
            var item = autoRenew
                ? new AutoRenewingCacheItem<TItem>(currentItem == null ? itemFunc() : currentItem.Item, lifespanMilliseconds ?? DefaultItemLifespanMilliseconds, itemFunc)
                : new TimedCacheItem<TItem>(currentItem == null ? itemFunc() : currentItem.Item, lifespanMilliseconds ?? DefaultItemLifespanMilliseconds);
            _cache.SetItem(key, item);
            return item;
        }

        #endregion

        #region Remove itens

        public bool RemoveItem(TKey key)
        {
            return _cache.RemoveItem(key);
        }

        public bool RemoveItem(TKey key, out object item)
        {
            return _cache.RemoveItem(key, out item);
        }

        #endregion

        public void Dispose()
        {
            if (_checkItemExpirationTimer == null) return;
            _checkItemExpirationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _checkItemExpirationTimer.Dispose();
        }
    }
}