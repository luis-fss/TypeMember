using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTests")]
namespace TypeMember.Internal
{
    /// <summary>
    /// Helper class for argument validation.
    /// </summary>
    internal static class Guard
    {
        /// <summary>
        /// Ensures the specified argument is not null.
        /// </summary>
        /// <param name="parameter">The parameter as an expression Ex.: () => parameter.</param>
        /// <param name="errorMessage">Optional error message.</param>
        public static void ShouldNotBeNull<T>(Expression<Func<T>> parameter, string errorMessage = null) where T : class
        {
            var body = (MemberExpression)parameter.Body;
            var value = ((FieldInfo)body.Member).GetValue(((ConstantExpression)body.Expression)?.Value);

            if (value is not null) return;

            var paramName = body.Member.Name;

            if (errorMessage is not null)
            {
                throw new ArgumentNullException(paramName, errorMessage);
            }

            throw new ArgumentNullException(paramName, $"The parameter '{paramName}' cannot be null.");
        }
    }
}