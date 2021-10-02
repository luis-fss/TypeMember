using System;
using System.Globalization;
using System.Linq;

namespace TypeMember
{
    public class ForType
    {
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
        public object ChangeType(object value, Type conversionType, CultureInfo cultureInfo = null)
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

        public object TryChangeType(object value, Type conversionType, CultureInfo cultureInfo = null)
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

        public bool IsNumeric(object obj)
        {
            return obj is not null && IsNumeric(obj.GetType());
        }

        public bool IsNumeric(Type type)
        {
            if (type is null)
            {
                return false;
            }

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
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.DateTime:
                case TypeCode.String:
                    return false;
                default:
                    return false;
            }
        }

        public bool IsPrimitive(Type t)
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
            }.Contains(t);
        }
    }
}