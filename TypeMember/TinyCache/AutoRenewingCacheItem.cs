using System;

namespace TypeMember.TinyCache
{
    internal class AutoRenewingCacheItem<TItem> : TimedCacheItem<TItem>
    {
        private readonly Func<TItem> _renewalFunction;
        public override bool HasExpired => false;

        public AutoRenewingCacheItem(TItem item, int lifespanMilliseconds, Func<TItem> renewalFunction)
            : base(item, lifespanMilliseconds)
        {
            _renewalFunction = renewalFunction;
        }

        public override TItem Item
        {
            get
            {
                if (base.HasExpired)
                {
                    base.Item = _renewalFunction();
                    ExpiryDateTime = DateTime.Now.AddMilliseconds(LifespanMilliseconds);
                }
                return base.Item;
            }
            protected set => base.Item = value;
        }
    }
}