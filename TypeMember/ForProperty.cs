using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMember.TinyCache;
using TypeMember.Util;

namespace TypeMember
{
    public class ForProperty
    {
        private static ITinyCache<string> Cache =>
            // 3600000 Milliseconds = 60 Minutes
            // 900000 Milliseconds = 15 Minutes
            //                         60 Min,  15 Min
            new TimedTinyCache<string>(3600000, 900000);
        
        public bool IsValidPropertyPath<T>(string propertyPath)
        {
            return typeof(T).IsValidPropertyPath(propertyPath);
        }

        public string GetPropertyPath<T>(Expression<Func<T, object>> expression, string collectionSuffix = null)
        {
            return expression.GetPropertyPath(collectionSuffix);
        }

        public HashSet<string> GetAllPropertiesPaths<T>()
        {
            return GetAllPropertiesPathsWithCache(typeof(T));
        }

        public bool IsNotPrimitive(Type t)
        {
            return new[] {
                typeof(string),
                typeof(char),
                typeof(byte),
                typeof(ushort),
                typeof(short),
                typeof(uint),
                typeof(int),
                typeof(ulong),
                typeof(long),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime)
            }.Contains(t) == false /*&& !t.IsInterface && !t.IsArray && t.IsClass && !t.Module.Name.Equals("mscorlib.dll")*/;
        }

        public string FixPathCase(Type type, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return null;

            var parts = propertyName.Split('.');

            if (parts.Length > 1)
            {
                var propertyInfo = type.GetProperty(parts[0], Reflector.DefaultBindings);
                return propertyInfo == null
                    ? null
                    : string.Concat(propertyInfo.Name, ".", FixPathCase(propertyInfo.PropertyType,
                        parts.Skip(1).Aggregate((a, i) => a + "." + i)));
            }

            var property = type.GetProperty(propertyName, Reflector.DefaultBindings);

            return property == null ? null : property.Name;
        }

        public string FixPathCase<TSource>(string propertyName)
        {
            return FixPathCase(typeof(TSource), propertyName);
        }

        public LambdaExpression GetPropertyExpression<TSource>(string propName, Type propType)
        {
            var getPropertyExpression = Reflector.MemberName.Get(() => GetPropertyExpression<object, object>(null));

            var method = typeof(Reflector).GetMethod(
                getPropertyExpression,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                Type.DefaultBinder,
                new[] { typeof(string) },
                null);

            var genericMethod = method?.MakeGenericMethod(typeof(TSource), propType);

            var lambdaExpression = genericMethod?.Invoke(null, new object[] { /*typeof(TSource).Name + "." +*/ propName }) as LambdaExpression;

            return lambdaExpression;
        }

        public Expression<Func<TSource, TProperty>> GetPropertyExpression<TSource, TProperty>(string propPath)
        {
            var split = propPath.Split('.');

            // Create the root of the expression, namely accessing an employee variable. Could be a Expression.Parameter too.
            var baseExpr = Expression.Parameter(typeof(TSource), typeof(TSource).Name.ToLowerInvariant());

            // Start at index 0 (the root)
            var result = BuildAccessors(baseExpr, split, 0);

            if (result is MemberExpression memberExpression && memberExpression.Type != typeof(TProperty) && memberExpression.Type.IsValueType)
            {
                result = Expression.Convert(result, typeof(TProperty));
            }

            // Create the resulting lambda
            var lambda = Expression.Lambda<Func<TSource, TProperty>>(result, baseExpr);

            return lambda;
        }

        private Expression BuildAccessors(Expression parent, IReadOnlyList<string> properties, int index)
        {
            if (index < properties.Count)
            {
                var member = properties[index];

                // If it's IEnumerable like Orders is, then we need to do something more complicated
                if (typeof(IEnumerable).IsAssignableFrom(parent.Type) && parent.Type != typeof(string))
                {
                    var enumerableType = parent.Type.GetGenericArguments().Single(); // input eg: Employee.Orders (type IList<Order>), output: type Order

                    var param = Expression.Parameter(enumerableType, enumerableType.Name.ToLowerInvariant()); // declare parameter for the lambda expression of Orders.Select(x => x.OrderID)

                    var lambdaBody = BuildAccessors(param, properties, index); // Recurse to build the inside of the lambda, so x => x.OrderID. 

                    var funcType = typeof(Func<,>).MakeGenericType(enumerableType, lambdaBody.Type); // Lambda is of type Func<Order, int> in the case of x => x.OrderID

                    var lambda = Expression.Lambda(funcType, lambdaBody, param);

                    // This part is messy, I want to find the method Enumerable.Select<Order, int>(..) but I don't think there's a more succinct way. Might be wrong.
                    var selectMethod = (from m in typeof(Enumerable).GetMethods()
                        where m.Name == "Select"
                              && m.IsGenericMethod
                        let parameters = m.GetParameters()
                        where parameters.Length == 2
                              && parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>)
                        select m).Single().MakeGenericMethod(enumerableType, lambdaBody.Type);

                    // Do Orders.Select(x => x.OrderID)
                    var invokeSelect = Expression.Call(null, selectMethod, parent, lambda);

                    return invokeSelect;
                }

                // Simply access a property like OrderID
                var newParent = Expression.PropertyOrField(parent, member);

                // Recurse
                return BuildAccessors(newParent, properties, ++index);
            }

            // Return the final expression once we're done recurring.
            return parent;
        }

        internal HashSet<string> GetAllPropertiesPathsWithCache(Type type)
        {
            var getAllPropertiesPaths = Cache.GetOrSetItem("getAllPropertiesPathsCacheKey-{93e7c805-2277-4c03-a82b-e7a4b54c8e94}",
                () => new ConcurrentDictionary<Type, HashSet<string>>());

            return getAllPropertiesPaths.GetOrAdd(type,
                tp => GetAllPropertiesPathsWithoutCache(tp, null));
        }

        private HashSet<string> GetAllPropertiesPathsWithoutCache(Type objectType, string rootPath)
        {
            if (!string.IsNullOrWhiteSpace(rootPath) && rootPath.StartsWith("."))
                rootPath = rootPath.Substring(1);

            var dicProperties = new HashSet<string>();
            var properties = objectType.GetProperties();

            foreach (var property in properties)
            {
                var val = property.PropertyType;
                if (property.PropertyType.IsClass == false || property.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    var path = string.IsNullOrWhiteSpace(rootPath) ? property.Name : $"{rootPath}.{property.Name}";
                    dicProperties.Add(path);
                }
                else
                {
                    dicProperties.UnionWith(GetAllPropertiesPathsWithoutCache(val, $"{rootPath}.{property.Name}"));
                }
            }

            return dicProperties;
        }
    }
}