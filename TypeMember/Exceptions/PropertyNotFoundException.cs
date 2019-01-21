using System;

namespace TypeMember.Exceptions
{
    /// <summary>
    /// Indicates that an expected property could not be found on a class 
    /// </summary>
    [Serializable]
    public class PropertyNotFoundException : Exception
    {
        public Type TargetType { get; private set; }
        public string PropertyName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:TelerikMvcGridCustomBindingHelper.Exceptions.PropertyNotFoundException"/> class, used when a property is missing. 
        /// </summary>
        /// <param name="targetType">The <see cref="T:System.Type"/> that is missing the property</param>
        /// <param name="propertyName">The name of the missing property</param>
        public PropertyNotFoundException(Type targetType, string propertyName)
            : base(string.Format("Could not find property nor field '{0}' in class '{1}'", propertyName, targetType))
        {
            TargetType = targetType;
            PropertyName = propertyName;
        }

        public PropertyNotFoundException(string msg, Type targetType)
            : base(msg)
        {
            TargetType = targetType;
        }
    }
}