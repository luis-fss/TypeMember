using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMember.Internal;

namespace TypeMember
{
    public class ForMemberInfo
    {
        public MemberInfo Get(Type type, string propertyName)
        {
            Guard.ShouldNotBeNull(() => type);
            Guard.ShouldNotBeNull(() => propertyName);

            var parts = propertyName.Split('.');

            if (parts.Length > 1)
            {
                MemberInfo Func(Type innerType) => Get(innerType, parts.Skip(1).Aggregate((a, i) => a + "." + i));

                var propertyInfo = type.GetProperty(parts[0], Reflector.DefaultBindings);
                if (propertyInfo != null)
                {
                    var t = ExtractUnderlyingTypeFromGenericEnumerable(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                    return Func(t);
                }

                var memberInfo = type.GetField(parts[0], Reflector.DefaultBindings);
                return memberInfo != null ? Func(memberInfo.FieldType) : null;
            }

            return (MemberInfo)type.GetProperty(propertyName, Reflector.DefaultBindings) ?? type.GetField(propertyName, Reflector.DefaultBindings);
        }

        public MemberInfo Get<TSource>(string propertyName)
        {
            return Get(typeof(TSource), propertyName);
        }

        public MemberInfo Get<TSource>(Expression<Func<TSource, object>> expression)
        {
            return Get((LambdaExpression)expression);
        }

        internal MemberInfo Get(LambdaExpression expression)
        {
            Guard.ShouldNotBeNull(() => expression);

            Expression expressionToCheck = expression;

            while (true)
            {
                switch (expressionToCheck.NodeType)
                {
                    case ExpressionType.Convert:
                        expressionToCheck = ((UnaryExpression)expressionToCheck).Operand;
                        break;
                    case ExpressionType.Lambda:
                        expressionToCheck = ((LambdaExpression)expressionToCheck).Body;
                        break;
                    case ExpressionType.MemberAccess:
                    {
                        var memberExpression = ((MemberExpression)expressionToCheck);
                        return memberExpression.Member;
                    }
                    case ExpressionType.Add:
                        expressionToCheck = ((BinaryExpression)expressionToCheck).Left;
                        break;
                    default:
                        throw new NotSupportedException($"This expression is not supported: {expression}");
                }
            }
        }

        private static Type ExtractUnderlyingTypeFromGenericEnumerable(Type type)
        {
            Guard.ShouldNotBeNull(() => type);

            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return interfaceType.GetGenericArguments()[0];
                }
            }
            return null;
        }
    }
}