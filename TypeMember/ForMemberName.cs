using System;
using System.Linq.Expressions;

namespace TypeMember
{
    public class ForMemberName
    {
        public string Get<TSource>(Expression<Func<TSource, object>> expression)
        {
            return Get(expression.Body);
        }

        public string Get<TSource>(Expression<Action<TSource>> expression)
        {
            return Get(expression.Body);
        }

        public string Get(Expression<Func<object>> expression)
        {
            return Get(expression.Body);
        }

        private string Get(Expression expression)
        {
            return expression switch
            {
                MemberExpression memberExpression => memberExpression.Member.Name,
                MethodCallExpression methodCallExpression => methodCallExpression.Method.Name,
                UnaryExpression unaryExpression => Get(unaryExpression),
                LambdaExpression lambdaExpression => Get(lambdaExpression.Body),
                _ => throw new NotSupportedException("Invalid expression")
            };
        }

        private string Get(UnaryExpression expression)
        {
            return expression.Operand switch
            {
                MethodCallExpression methodExpression => methodExpression.Method.Name,
                MemberExpression memberExpression => memberExpression.Member.Name,
                ConstantExpression constantExpression => constantExpression.Value?.ToString(),
                _ => throw new NotSupportedException("Invalid expression")
            };
        }
    }
}