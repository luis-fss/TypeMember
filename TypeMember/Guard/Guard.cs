using System;
using System.Linq.Expressions;
using System.Reflection;

namespace TypeMember.Guard
{
    /// <summary>
    /// Helper class for argument validation.
    /// </summary>
    public static class Guard
    {
        /// <summary>
        /// Ensures the specified argument is not null.
        /// </summary>
        /// <param name="parameter">The parameter as an expression Ex.: () => parameter.</param>
        /// <param name="errorMessage">Optional error message.</param>
        public static void IsNotNull<T>(Expression<Func<T>> parameter, string errorMessage = null) where T : class
        {
            var body = ((MemberExpression)parameter.Body);
            var value = ((FieldInfo)body.Member).GetValue(((ConstantExpression)body.Expression).Value);

            if (value != null) return;

            var paramName = body.Member.Name;

            if (errorMessage != null)
                throw new ArgumentNullException(paramName, errorMessage);

            throw new ArgumentNullException(paramName, $"The parameter {paramName} cannot be null.");
        }

        /// <summary>
        /// Ensures the specified string is not null nor empty.
        /// </summary>
        /// <param name="parameter">The parameter as an expression Ex.: () => parameter.</param>
        /// <param name="errorMessage">Optional error message.</param>
        public static void IsNotNullOrEmpty(Expression<Func<string>> parameter, string errorMessage = null)
        {
            IsNotNull(parameter, errorMessage);

            var body = ((MemberExpression)parameter.Body);
            var value = (string)((FieldInfo)body.Member).GetValue(((ConstantExpression)body.Expression).Value);

            if (string.IsNullOrWhiteSpace(value) == false) return;

            var paramName = body.Member.Name;

            if (errorMessage != null)
                throw new ArgumentException(paramName, errorMessage);

            throw new ArgumentException(paramName, $"The parameter {paramName} cannot be empty nor white space.");
        }

        public static GuardExpression If(bool execute)
        {
            return new GuardExpression(execute);
        }

        public static GuardExpression IfIsNull(object obj)
        {
            return new GuardExpression(obj == null);
        }
    }
}