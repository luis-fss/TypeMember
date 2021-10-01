using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMember.Exceptions;
using TypeMember.Util;

namespace TypeMember
{
    public static class Reflector
    {
        internal const BindingFlags DefaultBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase;

        public static ForMemberName MemberName { get; } = new();
        public static ForMemberInfo MemberInfo { get; } = new();
        public static ForMemberType MemberType { get; } = new();
        public static ForProperty Property { get; } = new();

        #region Change and convert types

        public static T As<T>(this object subject) where T : class
        {
            return subject as T;
        }

        public static T Cast<T>(this object subject)
        {
            return (T)subject;
        }

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
                case TypeCode.Empty:
                    break;
                case TypeCode.Object:
                    break;
                case TypeCode.DBNull:
                    break;
                case TypeCode.Boolean:
                    break;
                case TypeCode.Char:
                    break;
                case TypeCode.DateTime:
                    break;
                case TypeCode.String:
                    break;
                default:
                    return false;
            }

            return false;
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
    }
}