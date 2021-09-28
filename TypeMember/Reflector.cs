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
using TypeMember.TinyCache;
using TypeMember.Util;

namespace TypeMember
{
    public static class Reflector
    {
        private const BindingFlags DefaultBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase;

        private static ITinyCache<string> Cache =>
            // 3600000 Milliseconds = 60 Minutes
            // 900000 Milliseconds = 15 Minutes
            //                         60 Min,  15 Min
            new TimedTinyCache<string>(3600000, 900000);

        #region Get Member Name

        // ReSharper disable once UnusedParameter.Global
        public static string GetMemberName<TSource>(this TSource instance, Expression<Func<TSource, object>> expression)
        {
            return GetMemberName(expression);
        }

        public static string GetMemberName<TSource>(Expression<Func<TSource, object>> expression)
        {
            return GetMemberName(expression.Body);
        }

        // ReSharper disable once UnusedParameter.Global
        public static string GetMemberName<TSource>(this TSource instance, Expression<Action<TSource>> expression)
        {
            return GetMemberName(expression);
        }

        public static string GetMemberName<TSource>(Expression<Action<TSource>> expression)
        {
            return GetMemberName(expression.Body);
        }

        public static string GetMemberName(Expression<Func<object>> expression)
        {
            return GetMemberName(expression.Body);
        }

        private static string GetMemberName(Expression expression)
        {
            Guard.Guard.IsNotNull(() => expression);

            if (expression is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            if (expression is MethodCallExpression methodCallExpression)
            {
                return methodCallExpression.Method.Name;
            }

            if (expression is UnaryExpression unaryExpression)
            {
                return GetMemberName(unaryExpression);
            }

            if (expression is LambdaExpression lambdaExpression)
            {
                return GetMemberName(lambdaExpression.Body);
            }

            throw new ArgumentException("Invalid expression");
        }

        private static string GetMemberName(UnaryExpression expression)
        {
            Guard.Guard.IsNotNull(() => expression);

            if (expression.Operand is MethodCallExpression methodExpression)
                return methodExpression.Method.Name;

            if (expression.Operand is MemberExpression memberExpression)
                return memberExpression.Member.Name;

            if (expression.Operand is ConstantExpression constantExpression)
                return constantExpression.Value?.ToString();

            throw new ArgumentException("Invalid expression");
        }

        // ReSharper disable once UnusedParameter.Global
        public static List<string> GetMemberNames<TSource>(this TSource instance, params Expression<Func<TSource, object>>[] expressions)
        {
            return expressions.Select(GetMemberName).ToList();
        }

        #endregion

        #region Get Member Info

        public static MemberInfo GetMemberInfo(Type type, string propertyName)
        {
            Guard.Guard.IsNotNull(() => type);
            Guard.Guard.IsNotNull(() => propertyName);

            var parts = propertyName.Split('.');

            if (parts.Length > 1)
            {
                MemberInfo Func(Type innerType) => GetMemberInfo(innerType, parts.Skip(1).Aggregate((a, i) => a + "." + i));

                var propertyInfo = type.GetProperty(parts[0], DefaultBindings);
                if (propertyInfo != null)
                {
                    var t = ExtractUnderlyingTypeFromGenericEnumerable(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                    return Func(t);
                }

                var memberInfo = type.GetField(parts[0], DefaultBindings);
                return memberInfo != null ? Func(memberInfo.FieldType) : null;
            }

            return (MemberInfo)type.GetProperty(propertyName, DefaultBindings) ?? type.GetField(propertyName, DefaultBindings);
        }

        public static MemberInfo GetMemberInfo<TSource>(string propertyName)
        {
            return GetMemberInfo(typeof(TSource), propertyName);
        }

        public static MemberInfo GetMemberInfo<TSource>(Expression<Func<TSource, object>> expression)
        {
            return GetMemberInfo((LambdaExpression)expression);
        }

        private static MemberInfo GetMemberInfo(LambdaExpression expression)
        {
            Guard.Guard.IsNotNull(() => expression);

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
            Guard.Guard.IsNotNull(() => type);

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

        #endregion

        #region Fix Member Path Case

        public static string FixMemberPathCase(Type type, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return null;

            var parts = propertyName.Split('.');

            if (parts.Length > 1)
            {
                var propertyInfo = type.GetProperty(parts[0], DefaultBindings);
                return propertyInfo == null
                    ? null
                    : string.Concat(propertyInfo.Name, ".",
                        FixMemberPathCase(propertyInfo.PropertyType,
                            parts.Skip(1).Aggregate((a, i) => a + "." + i)));
            }

            var property = type.GetProperty(propertyName, DefaultBindings);

            return property == null ? null : property.Name;
        }

        public static string FixMemberPathCase<TSource>(string propertyName)
        {
            return FixMemberPathCase(typeof(TSource), propertyName);
        }

        #endregion

        public static T As<T>(this object subject) where T : class
        {
            return subject as T;
        }

        public static T Cast<T>(this object subject)
        {
            return (T)subject;
        }

        public static LambdaExpression GetPropertyExpression<TSource>(string propName, Type propType)
        {
            var getPropertyExpression = GetMemberName(() => GetPropertyExpression<object, object>(null));

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

        public static Expression<Func<TSource, TProperty>> GetPropertyExpression<TSource, TProperty>(string propPath)
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

        private static Expression BuildAccessors(Expression parent, string[] properties, int index)
        {
            if (index < properties.Length)
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

        #region Change and convert types

        /// <summary>
        /// Returns an Object with the specified Type and whose value is equivalent to the specified object.
        /// </summary>
        /// <param name="value">An Object that implements the IConvertible interface.</param>
        /// <param name="conversionType">The Type to which value is to be converted.</param>
        /// <param name="cultureInfo">CultureInfo to format values</param>
        /// <returns>An object whose Type is conversionType (or conversionType's underlying type if conversionType
        /// is Nullable&lt;&gt;) and whose value is equivalent to value. -or- a null reference, if value is a null
        /// reference and conversionType is not a value type.</returns>
        /// <remarks>
        /// This method exists as a workaround to System.Convert.ChangeType(Object, Type) which does not handle
        /// nullables as of version 2.0 (2.0.50727.42) of the .NET Framework. The idea is that this method will
        /// be deleted once Convert.ChangeType is updated in a future version of the .NET Framework to handle
        /// nullable types, so we want this to behave as closely to Convert.ChangeType as possible.
        /// This method was written by Peter Johnson at:
        /// http://aspalliance.com/author.aspx?uId=1026.
        /// </remarks>
        public static object ChangeType(object value, Type conversionType, CultureInfo cultureInfo = null)
        {
            cultureInfo ??= CultureInfo.InvariantCulture;

            // Note: This if block was taken from Convert.ChangeType as is, and is needed here since we're
            // checking properties on conversionType below.
            if (conversionType == null)
            {
                throw new ArgumentNullException(nameof(conversionType));
            }

            // If it's not a nullable type, just pass through the parameters to Convert.ChangeType
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // It's a nullable type, so instead of calling Convert.ChangeType directly which would throw a
                // InvalidCastException (per http://weblogs.asp.net/pjohnson/archive/2006/02/07/437631.aspx),
                // determine what the underlying type is
                // If it's null, it won't convert to the underlying type, but that's fine since nulls don't really
                // have a type--so just return null
                // Note: We only do this check if we're converting to a nullable type, since doing it outside
                // would diverge from Convert.ChangeType's behavior, which throws an InvalidCastException if
                // value is null and conversionType is a value type.
                if (value == null)
                {
                    return null;
                }

                // It's a nullable type, and not null, so that means it can be converted to its underlying type,
                // so overwrite the passed-in conversion type with this underlying type
                conversionType = Nullable.GetUnderlyingType(conversionType);
            }

            if (IsNumeric(conversionType))
            {
                if (value is string val)
                {
                    value = Convert.ToDouble(val, cultureInfo);
                }
                else
                {
                    value = Convert.ToDouble(value);
                }

                value = Convert.ToDecimal(string.Format(cultureInfo, "{0:R}", value), cultureInfo);
            }

            // Now that we've guaranteed conversionType is something Convert.ChangeType can handle (i.e. not a
            // nullable type), pass the call on to Convert.ChangeType
            // ReSharper disable once AssignNullToNotNullAttribute
            return Convert.ChangeType(value, conversionType);
        }

        public static object TryChangeType(object value, Type conversionType, CultureInfo cultureInfo = null)
        {
            try
            {
                return ChangeType(value, conversionType, cultureInfo);
            }
            catch
            {
                if (conversionType.IsValueType)
                    return Activator.CreateInstance(conversionType);

                return null;
            }
        }

        public static bool IsNumeric(object obj)
        {
            return obj != null && IsNumeric(obj.GetType());
        }

        public static bool IsNumeric(Type type)
        {
            if (type == null)
                return false;

            var typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
            }

            return false;
        }

        #endregion

        #region Get properties paths

        public static bool IsValidPropertyPath(this Type type, string propertyPath)
        {
            var memberInfo = GetMemberInfo(type, propertyPath);
            return memberInfo != null;
        }

        public static bool IsValidPropertyPath<T>(string propertyPath)
        {
            return IsValidPropertyPath(typeof(T), propertyPath);
        }

        public static string GetPropertyPath<T>(this T instance, Expression<Func<T, object>> expression)
        {
            return expression.GetPropertyPath();
        }

        public static string GetPropertyPath<T>(Expression<Func<T, object>> expression, string collectionSuffix = null)
        {
            return expression.GetPropertyPath(collectionSuffix);
        }

        public static HashSet<string> GetAllPropertiesPaths<T>()
        {
            return GetAllPropertiesPathsWithCache(typeof(T));
        }

        public static HashSet<string> GetAllPropertiesPaths(this object obj)
        {
            return GetAllPropertiesPathsWithCache(obj.GetType());
        }

        public static HashSet<string> GetAllPropertiesPaths(this Type type)
        {
            return GetAllPropertiesPathsWithCache(type);
        }

        private static HashSet<string> GetAllPropertiesPathsWithCache(Type type)
        {
            var getAllPropertiesPaths = Cache.GetOrSetItem("getAllPropertiesPathsCacheKey-{93e7c805-2277-4c03-a82b-e7a4b54c8e94}",
                () => new ConcurrentDictionary<Type, HashSet<string>>());

            return getAllPropertiesPaths.GetOrAdd(type,
                tp => GetAllPropertiesPathsWithoutCache(tp, null));
        }

        private static HashSet<string> GetAllPropertiesPathsWithoutCache(Type objectType, string rootPath)
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

        public static bool IsNotPrimitive(Type t)
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

        #endregion

        #region Get or Set Properties Values

        #region Public Methods

        /// <summary>
        /// Execute the "propertyPath" string on the "source" object
        /// </summary>
        /// <param name="source">Object the code should be executed against</param>
        /// <param name="propertyPath">Code that should be executed ex. 'Person.Age'</param>
        /// <returns>The result of execute propertyPath on source</returns>
        public static object GetPropertyValue(object source, string propertyPath)
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
        public static bool SetPropertyValue(object source, string propertyPath, object value, bool createIfNestedNull = true)
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

        /// <summary>
        /// Convert the object to the correct type
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="type">Type to convert to</param>
        /// <param name="typeConverter">TypeConverter</param>
        /// <returns>Converted value</returns>
        public static ConversionResult ConvertValue(object value, Type type, TypeConverter typeConverter)
        {
            var conversionResult = new ConversionResult { Success = false };
            if (value != null && type != null)
            {
                Type objectType = value.GetType();
                if (objectType == type || type.IsAssignableFrom(objectType))
                {
                    conversionResult.Success = true;
                    conversionResult.ConvertedValue = value;
                }
                else
                {
                    // If there is an explicit type converter use it
                    if (typeConverter != null && typeConverter.CanConvertFrom(objectType))
                    {
                        try
                        {
                            conversionResult.ConvertedValue = typeConverter.ConvertFrom(value);
                            conversionResult.Success = true;
                        }
                        catch (FormatException) { }
                        catch (Exception e)
                        {
                            if (!(e.InnerException is FormatException))
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

        public static bool HydrateProperty(this object source, string property)
        {
            var result = GetReflectorResult(source, property, true, true);

            return result.PreviousValue != null;
        }

        #endregion

        #region Private Methods

        private static ReflectorResult GetReflectorResult(object source, string propertyPath, bool getValue, bool createIfNestedNull)
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

                bool retrieveValue = getValue;
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

        private static TypeConverter GetTypeConverter(MemberInfo memberInfo, Type targetType)
        {
            var typeConverters = memberInfo.GetCustomAttributes(typeof(TypeConverterAttribute), true);
            if (typeConverters.Length > 0)
            {
                var typeConverterAttribute = (TypeConverterAttribute)typeConverters[0];
                var typeFromName = Type.GetType(typeConverterAttribute.ConverterTypeName);
                if ((typeFromName != null) && typeof(TypeConverter).IsAssignableFrom(typeFromName))
                {
                    return (TypeConverter)Activator.CreateInstance(typeFromName);
                }
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

        private static void ProcessProperty(ReflectorResult result, string property, bool retrieveValue, bool createIfNestedNull)
        {
            // This is just a regular property
            var propertyInfo = result.Value.GetType().GetProperty(property, DefaultBindings);
            if (propertyInfo != null)
            {
                var value = result.Value;
                if (retrieveValue)
                {
                    value = propertyInfo.GetValue(result.Value, null);
                    if (value == null && createIfNestedNull)
                    {
                        value = ObjectFactory.Create(propertyInfo.PropertyType);
                        SetPropertyValue(result.Value, propertyInfo.Name, value);
                    }
                }
                result.SetResult(value, propertyInfo);
            }
            else
            {
                // Maybe it is a field
                var fieldInfo = result.Value.GetType().GetField(property, DefaultBindings);

                if (fieldInfo != null)
                {
                    object value = result.Value;
                    if (retrieveValue)
                    {
                        value = fieldInfo.GetValue(result.Value);
                        if (value == null && createIfNestedNull)
                        {
                            value = ObjectFactory.Create(fieldInfo.FieldType);
                            SetPropertyValue(result.Value, fieldInfo.Name, value);
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

        #endregion

        #region Helper Classes

        private class ReflectorResult
        {
            public MemberInfo MemberInfo { get; private set; }
            public object PreviousValue { get; set; }
            public object Value { get; private set; }

            public ReflectorResult(object startValue)
            {
                SetResult(startValue, null);
            }

            public void SetResult(object value, MemberInfo memberInfo)
            {
                Value = value;
                MemberInfo = memberInfo;
            }

            public void Clear()
            {
                MemberInfo = null;
                Value = null;
                PreviousValue = null;
            }
        }

        public class ConversionResult
        {
            public bool Success { get; set; }

            public object ConvertedValue { get; set; }
        }

        #endregion

        #endregion

        public static Type GetMemberType(MemberInfo memberInfo)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
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
                var memberInfo = GetMemberInfo(lambdaExpression);
                return GetMemberType(memberInfo);
            }

            return null;
        }
    }
}