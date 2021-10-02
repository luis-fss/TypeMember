using System.Reflection;

namespace TypeMember
{
    public static class Reflector
    {
        internal const BindingFlags DefaultBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase;

        public static ForMemberName MemberName { get; } = new();
        public static ForMemberInfo MemberInfo { get; } = new();
        public static ForMemberType MemberType { get; } = new();
        public static ForProperty Property { get; } = new();
        public static ForType Type { get; } = new();
    }
}