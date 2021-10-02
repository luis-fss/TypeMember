using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMember.Exceptions;
using TypeMember.Internal;
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
        
        public bool IsValidPath<T>(string propertyPath)
        {
            return typeof(T).IsValidPropertyPath(propertyPath);
        }

        public string GetPath<T>(Expression<Func<T, object>> expression, string collectionSuffix = null)
        {
            return expression.GetPropertyPath(collectionSuffix);
        }

        public HashSet<string> GetAllPaths<T>()
        {
            return GetAllPathsWithCache(typeof(T));
        }

        public HashSet<string> GetAllPaths(Type type)
        {
            return GetAllPathsWithCache(type);
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

        public LambdaExpression GetExpression<TSource>(string propName, Type propType)
        {
            var getPropertyExpression = Reflector.MemberName.Get(() => GetExpression<object, object>(null));

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

        public Expression<Func<TSource, TProperty>> GetExpression<TSource, TProperty>(string propPath)
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

        /// <summary>
        /// Execute the "propertyPath" string on the "source" object
        /// </summary>
        /// <param name="source">Object the code should be executed against</param>
        /// <param name="propertyPath">Code that should be executed ex. 'Person.Age'</param>
        /// <returns>The result of execute propertyPath on source</returns>
        public object GetValue(object source, string propertyPath)
        {
            var reflectorResult = GetReflectorResult(source, propertyPath, true, false);
            return reflectorResult?.Value;
        }

        /// <summary>
        /// Sets the "source" object to the "value" specified in "propertyPath"
        /// </summary>
        /// <param name="source">Object the code should be executed against</param>
        /// <param name="propertyPath">Code that should be executed ex. 'Person.Age'</param>
        /// <param name="value">Value to set the source+propertyPath to.</param>
        /// <param name="createIfNestedNull">Creates items it cannot find</param>
        public bool SetValue(object source, string propertyPath, object value, bool createIfNestedNull = true)
        {
            var success = true;

            var reflectorResult = GetReflectorResult(source, propertyPath, false, createIfNestedNull);
            if (reflectorResult != null)
            {
                TypeConverter typeConverter;
                var propertyInfo = reflectorResult.MemberInfo as PropertyInfo;
                if (propertyInfo != null)
                {
                    if (propertyInfo.CanWrite)
                    {
                        typeConverter = GetTypeConverter(propertyInfo);

                        var conversionResult = ConvertValue(value, propertyInfo.PropertyType, typeConverter);
                        if (conversionResult.Success)
                        {
                            propertyInfo.SetValue(reflectorResult.PreviousValue, conversionResult.ConvertedValue, null);
                        }
                        else
                        {
                            //Invalid value
                            success = false;
                        }
                    }
                }
                else
                {
                    var fieldInfo = reflectorResult.MemberInfo as FieldInfo;
                    if (fieldInfo != null)
                    {
                        typeConverter = GetTypeConverter(fieldInfo);
                        var conversionResult = ConvertValue(value, fieldInfo.FieldType, typeConverter);
                        if (conversionResult.Success)
                        {
                            fieldInfo.SetValue(reflectorResult.PreviousValue, conversionResult.ConvertedValue);
                        }
                        else
                        {
                            //Invalid value
                            success = false;
                        }
                    }
                    else
                    {
                        // both property and field are invalid
                        success = false;
                    }
                }
            }
            else
            {
                success = false;
            }

            return success;
        }

        internal ReflectorResult GetReflectorResult(object source, string propertyPath, bool getValue, bool createIfNestedNull)
        {
            var result = new ReflectorResult(source);

            // Split the code into usable fragments
            var fragments = propertyPath.Split('.');
            for (var i = 0; i < fragments.Length; i++)
            {
                // if the value is null we cannot go any deeper so don't waste your time
                if (result.Value == null)
                {
                    return result;
                }

                var retrieveValue = getValue;
                
                if (retrieveValue == false)
                {
                    // If this is not the last one in the array, get it anyway
                    retrieveValue = i + 1 != fragments.Length;
                }

                var property = fragments[i];
                result.PreviousValue = result.Value;
                ProcessProperty(result, property, retrieveValue, createIfNestedNull);
            }

            return result;
        }

        private void ProcessProperty(ReflectorResult result, string property, bool retrieveValue, bool createIfNestedNull)
        {
            // This is just a regular property
            var propertyInfo = result.Value.GetType().GetProperty(property, Reflector.DefaultBindings);
            if (propertyInfo is not null)
            {
                var value = result.Value;
                if (retrieveValue)
                {
                    value = propertyInfo.GetValue(result.Value, null);
                    if (value == null && createIfNestedNull)
                    {
                        value = ObjectFactory<object>.Create(propertyInfo.PropertyType);
                        SetValue(result.Value, propertyInfo.Name, value);
                    }
                }
                result.SetResult(value, propertyInfo);
            }
            else
            {
                // Maybe it is a field
                var fieldInfo = result.Value.GetType().GetField(property, Reflector.DefaultBindings);

                if (fieldInfo is not null)
                {
                    var value = result.Value;
                    if (retrieveValue)
                    {
                        value = fieldInfo.GetValue(result.Value);
                        if (value == null && createIfNestedNull)
                        {
                            value = ObjectFactory<object>.Create(fieldInfo.FieldType);
                            SetValue(result.Value, fieldInfo.Name, value);
                        }
                    }
                    result.SetResult(value, fieldInfo);
                }
                else
                {
                    // This item is missing, log it and set the value to null
                    var type = result.Value.GetType();
                    result.Clear();
                    throw new PropertyNotFoundException(type, property);
                }
            }
        }

        /// <summary>
        /// Convert the object to the correct type
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="type">Type to convert to</param>
        /// <param name="typeConverter">TypeConverter</param>
        /// <returns>Converted value</returns>
        private static ConversionResult ConvertValue(object value, Type type, TypeConverter typeConverter)
        {
            var conversionResult = new ConversionResult { Success = false };
            if (value is not null && type is not null)
            {
                var objectType = value.GetType();
                if (objectType == type || type.IsAssignableFrom(objectType))
                {
                    conversionResult.ConvertedValue = value;
                    conversionResult.Success = true;
                }
                else
                {
                    // If there is an explicit type converter use it
                    if (typeConverter is not null && typeConverter.CanConvertFrom(objectType))
                    {
                        try
                        {
                            conversionResult.ConvertedValue = typeConverter.ConvertFrom(value);
                            conversionResult.Success = true;
                        }
                        catch (FormatException) { }
                        catch (Exception e)
                        {
                            if (e.InnerException is not FormatException)
                            {
                                throw;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            conversionResult.ConvertedValue = Convert.ChangeType(value, type, CultureInfo.CurrentCulture);
                            conversionResult.Success = true;
                        }
                        catch (InvalidCastException) { }
                    }
                }
            }
            return conversionResult;
        }

        private static Expression BuildAccessors(Expression parent, IReadOnlyList<string> properties, int index)
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

        private static HashSet<string> GetAllPathsWithCache(Type type)
        {
            var getAllPropertiesPaths = Cache.GetOrSetItem("getAllPropertiesPathsCacheKey-{93e7c805-2277-4c03-a82b-e7a4b54c8e94}",
                () => new ConcurrentDictionary<Type, HashSet<string>>());

            return getAllPropertiesPaths.GetOrAdd(type,
                tp => GetAllPathsWithoutCache(tp, null));
        }

        private static HashSet<string> GetAllPathsWithoutCache(Type objectType, string rootPath)
        {
            if (!string.IsNullOrWhiteSpace(rootPath) && rootPath.StartsWith("."))
            {
                rootPath = rootPath[1..];
            }

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
                    dicProperties.UnionWith(GetAllPathsWithoutCache(val, $"{rootPath}.{property.Name}"));
                }
            }

            return dicProperties;
        }

        private static TypeConverter GetTypeConverter(MemberInfo memberInfo, Type targetType)
        {
            var typeConverters = memberInfo.GetCustomAttributes(typeof(TypeConverterAttribute), true);

            if (typeConverters.Length <= 0)
            {
                return TypeDescriptor.GetConverter(targetType);
            }

            var typeConverterAttribute = (TypeConverterAttribute)typeConverters[0];
            var typeFromName = Type.GetType(typeConverterAttribute.ConverterTypeName);
                
            if (typeFromName is not null && typeof(TypeConverter).IsAssignableFrom(typeFromName))
            {
                return (TypeConverter)Activator.CreateInstance(typeFromName);
            }

            return TypeDescriptor.GetConverter(targetType);
        }

        private static TypeConverter GetTypeConverter(PropertyInfo propertyInfo)
        {
            return GetTypeConverter(propertyInfo, propertyInfo.PropertyType);
        }

        private static TypeConverter GetTypeConverter(FieldInfo fieldInfo)
        {
            return GetTypeConverter(fieldInfo, fieldInfo.FieldType);
        }
    }
}