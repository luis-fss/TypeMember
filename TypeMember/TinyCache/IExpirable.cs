using System;

namespace TypeMember.TinyCache
{
    interface IExpirable
    {
        int LifespanMilliseconds { get; }
        DateTime ExpiryDateTime { get; }
        bool HasExpired { get; }
    }
}