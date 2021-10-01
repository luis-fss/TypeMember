using System;
using System.Linq.Expressions;
using System.Reflection;

namespace TypeMember
{
    public class ForMemberType
    {
        public static Type GetMemberType(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.PropertyType;
            }

            var fieldInfo = memberInfo as FieldInfo;
            return fieldInfo?.FieldType;
        }

        public static Type GetMemberType(Expression expression)
        {
            if (expression is LambdaExpression lambdaExpression)
            {
                var memberInfo = Reflector.MemberInfo.Get(lambdaExpression);
                return GetMemberType(memberInfo);
            }

            return null;
        }
    }
}