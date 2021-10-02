using System.Reflection;

namespace TypeMember.Internal
{
    internal class ReflectorResult
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
}