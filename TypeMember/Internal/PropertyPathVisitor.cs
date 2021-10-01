using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TypeMember.Util;

namespace TypeMember.Internal
{
    internal class PropertyPathVisitor : BasePropertyPathVisitor
    {
        public string CollectionSuffix { get; init; }

        public HashSet<string> Properties { get; private set; }

        private readonly bool _flag;
        private Type _underlyingRootType;

        public PropertyPathVisitor() : base(null)
        {
            _flag = true;
            Properties = new HashSet<string>();
        }

        private PropertyPathVisitor(Expression expression, bool flag) : base(expression)
        {
            _flag = flag;
        }

        public override Expression Visit(Expression node)
        {
            try
            {
                if (node is LambdaExpression lambda)
                {
                    _underlyingRootType ??= lambda.Parameters[0].Type;
                }
                return base.Visit(node);
            }
            catch (Exception ex)
            {
                throw new NotSupportedException($"This expression is not supported: {UnderlyingExpression ?? node}", ex);
            }
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (_flag)
            {
                var propertyPath = new PropertyPathVisitor(node, false).GetPropertyPath();

                var ifNotContainPath = Properties.FirstOrDefault(s => s.StartsWith(propertyPath)) == null;

                if (ifNotContainPath)
                {
                    if (Properties.Count > 0)
                    {
                        var element = Properties.ElementAt(Properties.Count - 1);
                        if (_underlyingRootType != null && _underlyingRootType.IsValidPropertyPath(element) == false)
                        {
                            propertyPath = $"{propertyPath}{CollectionSuffix}.{element}";
                            Properties.Remove(element);
                        }
                        //else if (_underlyingRootType != null && _underlyingRootType.IsValidPropertyPath(propertyPath) == false)
                        //{
                        //    //throw new NotSupportedException(string.Format("This expression is not supported: {0}", UnderlyingExpression ?? node));
                        //}
                    }

                    Properties.Add(propertyPath);
                }
            }

            return base.VisitMember(node);
        }

        public void Reset()
        {
            Properties = new HashSet<string>();
        }
    }
}