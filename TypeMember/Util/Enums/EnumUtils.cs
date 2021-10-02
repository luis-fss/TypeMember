using System;
using System.Collections;
using System.Collections.Concurrent;

namespace TypeMember.Util.Enums
{
    /// <summary>
    /// Helper class for working with 'extended' enums using <see cref="StringValueAttribute"/> attributes.
    /// </summary>
    public class EnumUtils
    {
        #region Instance implementation

        private readonly Type _enumType;
        //private static readonly Hashtable StringValues = Hashtable.Synchronized(new Hashtable());
        private static readonly ConcurrentDictionary<Enum, StringValueAttribute> StringValues = new();

        /// <summary>
        /// Creates a new <see cref="EnumUtils"/> instance.
        /// </summary>
        /// <param name="enumType">Enum type.</param>
        public EnumUtils(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException($"Supplied type must be an Enum.  Type was {enumType}");

            _enumType = enumType;
        }

        /// <summary>
        /// Gets the string value associated with the given enum value.
        /// </summary>
        /// <param name="valueName">Name of the enum value.</param>
        /// <returns>String Value</returns>
        public string GetStringValue(string valueName)
        {
            string stringValue;
            try
            {
                var enumType = (Enum)Enum.Parse(_enumType, valueName);
                stringValue = GetStringValue(enumType);
            }
            catch
            {
                //Swallow!
                return null;
            }

            return stringValue;
        }

        /// <summary>
        /// Gets the string values associated with the enum.
        /// </summary>
        /// <returns>String value array</returns>
        public Array GetStringValues()
        {
            var values = new ArrayList();
            //Look for our string value associated with fields in this enum
            foreach (var fi in _enumType.GetFields())
            {
                //Check for our custom attribute
                if (fi.GetCustomAttributes(typeof(StringValueAttribute), false) is StringValueAttribute[] { Length: > 0 } attrs)
                {
                    values.Add(attrs[0].Value);
                }
            }

            return values.ToArray();
        }

        /// <summary>
        /// Gets the values as a 'bindable' list datasource.
        /// </summary>
        /// <returns>IList for data binding</returns>
        public IList GetListValues()
        {
            var underlyingType = Enum.GetUnderlyingType(_enumType);
            var values = new ArrayList();
            //Look for our string value associated with fields in this enum
            foreach (var fi in _enumType.GetFields())
            {
                //Check for our custom attribute
                if (fi.GetCustomAttributes(typeof(StringValueAttribute), false) is StringValueAttribute[] { Length: > 0 } attrs)
                {
                    values.Add(new DictionaryEntry(Convert.ChangeType(Enum.Parse(_enumType, fi.Name), underlyingType), attrs[0].Value));
                }
            }

            return values;

        }

        /// <summary>
        /// Return the existence of the given string value within the enum.
        /// </summary>
        /// <param name="stringValue">String value.</param>
        /// <returns>Existence of the string value</returns>
        public bool IsStringDefined(string stringValue)
        {
            return Parse(_enumType, stringValue) != null;
        }

        /// <summary>
        /// Return the existence of the given string value within the enum.
        /// </summary>
        /// <param name="stringValue">String value.</param>
        /// <param name="ignoreCase">Denotes whether to conduct a case-insensitive match on the supplied string value</param>
        /// <returns>Existence of the string value</returns>
        public bool IsStringDefined(string stringValue, bool ignoreCase)
        {
            return Parse(_enumType, stringValue, ignoreCase) != null;
        }

        /// <summary>
        /// Gets the underlying enum type for this instance.
        /// </summary>
        /// <value></value>
        public Type EnumType => _enumType;

        #endregion

        #region Static implementation

        /// <summary>
        /// Gets a string value for a particular enum value.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <returns>String Value associated via a <see cref="StringValueAttribute"/> attribute, or null if not found.</returns>
        public static string GetStringValue(Enum value)
        {
            string output = null;
            var type = value.GetType();

            if (StringValues.ContainsKey(value))
            {
                var stringValueAttribute = StringValues[value];
                if (stringValueAttribute != null)
                {
                    output = stringValueAttribute.Value;
                }
            }
            else
            {
                //Look for our 'StringValueAttribute' in the field's custom attributes
                // ReSharper disable SpecifyACultureInStringConversionExplicitly
                var fi = type.GetField(value.ToString());
                // ReSharper restore SpecifyACultureInStringConversionExplicitly
                if (fi?.GetCustomAttributes(typeof(StringValueAttribute), false) is StringValueAttribute[] { Length: > 0 } attrs)
                {
                    StringValues.TryAdd(value, attrs[0]);
                    output = attrs[0].Value;
                }

            }
            
            return output;
        }

        /// <summary>
        /// Parses the supplied enum and string value to find an associated enum value.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="stringValue">String value.</param>
        /// <param name="ignoreCase">Denotes whether to conduct a case-insensitive match on the supplied string value</param>
        /// <returns>Enum value associated with the string value, or null if not found.</returns>
        public static object Parse(Type type, string stringValue, bool ignoreCase = false)
        {
            string enumStringValue = null;

            if (!type.IsEnum)
                throw new ArgumentException($"Supplied type must be an Enum.  Type was {type}");

            //Look for our string value associated with fields in this enum
            foreach (var fi in type.GetFields())
            {
                //Check for our custom attribute
                if (fi.GetCustomAttributes(typeof(StringValueAttribute), false) is StringValueAttribute[] { Length: > 0 } attrs)
                {
                    enumStringValue = attrs[0].Value;
                }

                //Check for equality then select actual enum value.
                if (string.Compare(enumStringValue, stringValue, ignoreCase) != 0) continue;
                return Enum.Parse(type, fi.Name);
            }

            return null;
        }

        /// <summary>
        /// Return the existence of the given string value within the enum.
        /// </summary>
        /// <param name="stringValue">String value.</param>
        /// <param name="enumType">Type of enum</param>
        /// <returns>Existence of the string value</returns>
        public static bool IsStringDefined(Type enumType, string stringValue)
        {
            return Parse(enumType, stringValue) != null;
        }

        /// <summary>
        /// Return the existence of the given string value within the enum.
        /// </summary>
        /// <param name="stringValue">String value.</param>
        /// <param name="enumType">Type of enum</param>
        /// <param name="ignoreCase">Denotes whether to conduct a case-insensitive match on the supplied string value</param>
        /// <returns>Existence of the string value</returns>
        public static bool IsStringDefined(Type enumType, string stringValue, bool ignoreCase)
        {
            return Parse(enumType, stringValue, ignoreCase) != null;
        }

        #endregion
    }
}