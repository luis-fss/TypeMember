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
            return (string.IsNullOrWhiteSpace(str) && string.IsNullOrEmpty(str)) == false;
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return (string.IsNullOrWhiteSpace(str) && string.IsNullOrEmpty(str));
        }

        public static string GetPropertyPath(this Expression expression)
        {
            var hashSet = GetAllPropertiesPaths(expression) ?? new HashSet<string>();
            return hashSet.Any() ? hashSet.ElementAt(0) : null;
        }

        public static HashSet<string> GetAllPropertiesPaths(this Expression expression)
        {
            if (expression == null)
                return null;

            var visitor = new PropertyPathVisitor();
            visitor.Visit(expression);
            return visitor.Properties;
        }

        public static string GetStringValue(this Enum @enum)
        {
            return EnumUtils.GetStringValue(@enum);
        }

        public static bool NotEquals(this string src, string other)
        {
            return src.Equals(other) == false;
        }
    }
}