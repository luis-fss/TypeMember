using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace TypeMember.Util
{
    public abstract class BasePropertyPathVisitor : ExpressionVisitor
    {
        private readonly Stack<string> _stack;
        protected readonly Expression UnderlyingExpression;
        private bool _processed;

        protected BasePropertyPathVisitor(Expression expression)
        {
            UnderlyingExpression = expression;
            _stack = new Stack<string>();
        }

        protected string GetPropertyPath()
        {
            if (_processed == false)
            {
                Visit(UnderlyingExpression);
            }

            var result = new StringBuilder();

            result = _stack.Aggregate(result, (current, s) => (current.Length > 0 ? current.Append(".") : current).Append(s));

            return result.ToString();
        }

        public override Expression Visit(Expression node)
        {
            _processed = true;
            return base.Visit(node);
        }

        protected override Expression VisitMember(MemberExpression expression)
        {
            _stack?.Push(expression.Member.Name);
            return base.VisitMember(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            if (IsLinqOperator(expression.Method))
            {
                for (var i = 1; i < expression.Arguments.Count; i++)
                {
                    Visit(expression.Arguments[i]);
                }
                Visit(expression.Arguments[0]);
                return expression;
            }
            return base.VisitMethodCall(expression);
        }

        private static bool IsLinqOperator(MemberInfo method)
        {
            if (method.DeclaringType != typeof(Queryable) && method.DeclaringType != typeof(Enumerable))
                return false;
            return Attribute.GetCustomAttribute(method, typeof(ExtensionAttribute)) != null;
        }
    }
}