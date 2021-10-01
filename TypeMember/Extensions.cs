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
    }
}