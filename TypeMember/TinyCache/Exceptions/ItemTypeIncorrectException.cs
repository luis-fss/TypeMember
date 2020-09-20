using System;

namespace TypeMember.TinyCache.Exceptions
{
    public class ItemTypeIncorrectException : Exception
    {
        public ItemTypeIncorrectException(object key, Type requestedType, Type actualType)
            : base("The item was not of the requested type")
        {
            Key = key;
            RequestedType = requestedType;
            ActualType = actualType;
        }

        public object Key { get; }
        public Type RequestedType { get; }
        public Type ActualType { get; }
    }
}