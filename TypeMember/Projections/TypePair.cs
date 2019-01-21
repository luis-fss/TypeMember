using System;

namespace TypeMember.Projections
{
    public struct TypePair2 : IEquatable<TypePair2>
    {
        public Type SourceType { get; private set; }

        public Type DestinationType { get; private set; }

        private readonly string _simpleEquatableString;

        public TypePair2(Type sourceType, Type destinationType, ISimpleEquatable simpleEquatable = null)
            : this()
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            if (simpleEquatable != null)
                _simpleEquatableString = string.Format("Hashcode: {0}\n String: {1}", simpleEquatable.GetHashCode(), simpleEquatable.Stringfy());
        }

        #region Equality members

        public bool Equals(TypePair2 other)
        {
            return SourceType == other.SourceType
                   && DestinationType == other.DestinationType
                   && Equals(_simpleEquatableString, other._simpleEquatableString);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TypePair2 && Equals((TypePair2)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SourceType.GetHashCode();
                hashCode = (hashCode * 397) ^ DestinationType.GetHashCode();
                if (_simpleEquatableString != null)
                {
                    hashCode = (hashCode * 397) ^ _simpleEquatableString.GetHashCode();
                }
                return hashCode;
            }
        }

        #endregion
    }
}