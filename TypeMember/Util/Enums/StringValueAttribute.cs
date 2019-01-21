using System;

namespace TypeMember.Util.Enums
{
    /// <summary>
    /// Simple attribute class for storing String Values
    /// </summary>
    public class StringValueAttribute : Attribute
    {
        private readonly string _value;

        /// <summary>
        /// Creates a new <see cref="StringValueAttribute"/> instance.
        /// </summary>
        /// <param name="value">Value.</param>
        public StringValueAttribute(string value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value></value>
        public string Value
        {
            get { return _value; }
        }
    }
}