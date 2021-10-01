using System;

namespace TypeMember.TinyCache
{
    internal class TimedCacheItem<TItem> : IExpirable
    {
        public virtual TItem Item { get; protected set; }
        public int LifespanMilliseconds { get; }
        public DateTime ExpiryDateTime { get; protected set; }
        public virtual bool HasExpired => ExpiryDateTime <= DateTime.Now;

        public TimedCacheItem(TItem item, int lifespanMilliseconds)
        {
            if (lifespanMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lifespanMilliseconds), "lifespanMilliseconds must have a positive value");
            }
            // ReSharper disable once VirtualMemberCallInConstructor
            Item = item;
            LifespanMilliseconds = lifespanMilliseconds;
            ExpiryDateTime = DateTime.Now.AddMilliseconds(lifespanMilliseconds);
        }
    }
}