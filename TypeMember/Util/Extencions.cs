using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TypeMember.Util.Enums;

namespace TypeMember.Util
{
    public static class Extencions
    {
        public static bool IsNotNullOrWhiteSpace(this string str)
        {
            return str.IsNullOrWhiteSpace() == false;
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str) && string.IsNullOrEmpty(str);
        }

        public static string GetStringValue(this Enum @enum)
        {
            return EnumUtils.GetStringValue(@enum);
        }

        public static bool NotEquals(this string src, string other)
        {
            return string.Equals(src, other) == false;
        }

        public static string GetPropertyPath(this Expression expression, string collectionSuffix = null)
        {
            var hashSet = GetAllPropertiesPaths(expression, collectionSuffix) ?? new HashSet<string>();
            return hashSet.Any() ? hashSet.ElementAt(0) : null;
        }

        public static HashSet<string> GetAllPropertiesPaths(this Expression expression, string collectionSuffix = null)
        {
            if (expression == null)
                return null;

            var visitor = new PropertyPathVisitor();
            visitor.CollectionSuffix = collectionSuffix;
            visitor.Visit(expression);
            return visitor.Properties;
        }
    }
}