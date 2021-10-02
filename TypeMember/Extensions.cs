using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace TypeMember
{
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public static class Extensions
    {
        public static string GetMemberName<TSource>(this TSource instance, Expression<Func<TSource, object>> expression)
        {
            return Reflector.MemberName.Get(expression);
        }

        public static string GetMemberName<TSource>(this TSource instance, Expression<Action<TSource>> expression)
        {
            return Reflector.MemberName.Get(expression);
        }
        
        public static List<string> GetMemberNames<TSource>(this TSource instance, params Expression<Func<TSource, object>>[] expressions)
        {
            return expressions.Select(Reflector.MemberName.Get).ToList();
        }
        
        public static bool IsValidPropertyPath(this Type type, string propertyPath)
        {
            var memberInfo = Reflector.MemberInfo.Get(type, propertyPath);
            return memberInfo is not null;
        }

        public static string GetPropertyPath<T>(this T instance, Expression<Func<T, object>> expression)
        {
            return Reflector.Property.GetPath(expression);
        }

        public static HashSet<string> GetAllPropertiesPaths(this object obj)
        {
            return Reflector.Property.GetAllPaths(obj.GetType());
        }

        public static HashSet<string> GetAllPropertiesPaths(this Type type)
        {
            return Reflector.Property.GetAllPaths(type);
        }

        public static bool HydrateProperty(this object source, string property)
        {
            var result = Reflector.Property.GetReflectorResult(source, property, true, true);

            return result.PreviousValue is not null;
        }
        
        public static T As<T>(this object subject) where T : class
        {
            return subject as T;
        }

        public static T Cast<T>(this object subject)
        {
            return (T)subject;
        }
    }
}